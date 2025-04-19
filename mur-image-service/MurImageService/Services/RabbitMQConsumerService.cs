// Service consommateur (MurImageService)
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MurImageService.Messages;
using MurImageService.Models;
using Prometheus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MurImageService.Services;
public class RabbitMQConsumerService : BackgroundService
{
    private IConnection _connection;
    private IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private const string ExchangeName = "camera_events";
    private const string QueueName = "camera_status_changes";
    private const string RoutingKey = "camera.status.changed";
    
    // Métriques Prometheus
    private static readonly Counter MessageReceivedCounter = Metrics
        .CreateCounter("mur_image_messages_received_total", "Total number of messages received from RabbitMQ", 
            new CounterConfiguration 
            { 
                LabelNames = new[] { "camera_code", "status" } 
            });
            
    private static readonly Counter PositionUpdatesCounter = Metrics
        .CreateCounter("mur_image_position_updates_total", "Total number of camera position updates performed");
        
    private static readonly Gauge ActivePositionsGauge = Metrics
        .CreateGauge("mur_image_active_positions", "Number of active camera positions");
        
    private static readonly Histogram MessageProcessingTime = Metrics
        .CreateHistogram("mur_image_message_processing_seconds", 
            "Histogram of message processing durations",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
            });

    public RabbitMQConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RabbitMQConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service consommateur RabbitMQ démarré");
        
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Déclarer l'exchange et la queue
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
            
            _logger.LogInformation("Connexion établie avec RabbitMQ");
            
            // Initialiser la métrique des positions actives
            InitializeActivePositionMetric();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion à RabbitMQ");
            throw;
        }
        
        return base.StartAsync(cancellationToken);
    }

    private void InitializeActivePositionMetric()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MurImageDbContext>();
            
            var activePositionCount = dbContext.Positions
                .Count(p => p.CodeCamera != null && p.EstActif);
                
            ActivePositionsGauge.Set(activePositionCount);
            
            _logger.LogInformation($"Métrique des positions actives initialisée: {activePositionCount} positions actives");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de la métrique des positions actives");
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (sender, args) =>
            {
                try
                {
                    using (MessageProcessingTime.NewTimer())
                    {
                        var body = args.Body.ToArray();
                        var messageJson = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<CameraStatusChangedMessage>(messageJson);
                        
                        if (message != null)
                        {
                            MessageReceivedCounter
                                .WithLabels(message.CameraCode, message.EstAfficher ? "activated" : "deactivated")
                                .Inc();
                                
                            await HandleCameraStatusChanged(message);
                        }
                        
                        _channel.BasicAck(args.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors du traitement du message");
                    _channel.BasicNack(args.DeliveryTag, false, true);
                }
            };
            
            _channel.BasicConsume(QueueName, false, consumer);
            
            _logger.LogInformation("Consommateur de messages configuré");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la configuration du consommateur");
            return Task.CompletedTask;
        }
    }

    private async Task HandleCameraStatusChanged(CameraStatusChangedMessage message)
    {
        _logger.LogInformation($"Message reçu: Caméra {message.CameraCode} est {(message.EstAfficher ? "affichée" : "masquée")} à {message.Timestamp}");
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MurImageDbContext>();

            // Trouver toutes les positions qui utilisent cette caméra
            var positions = await dbContext.Positions
                .Where(p => p.CodeCamera == message.CameraCode)
                .ToListAsync();

            _logger.LogInformation($"Positions trouvées pour la caméra {message.CameraCode}: {positions.Count}");
            
            // Mettre à jour l'état EstActif de chaque position selon l'état de la caméra
            bool positionsUpdated = false;
            foreach (var position in positions)
            {
                // Si l'état change
                if (position.EstActif != message.EstAfficher)
                {
                    // Ajuster le compteur de positions actives
                    if (message.EstAfficher)
                    {
                        ActivePositionsGauge.Inc();
                    }
                    else
                    {
                        ActivePositionsGauge.Dec();
                    }
                }
                
                position.EstActif = message.EstAfficher;
                _logger.LogInformation($"Position {position.Id} du mur d'image {position.IdMurImage} mise à jour: EstActif = {position.EstActif}");
                positionsUpdated = true;
            }
            
            if (positionsUpdated)
            {
                // Incrémenter le compteur de mises à jour
                PositionUpdatesCounter.Inc(positions.Count);
                
                // Sauvegarder les changements dans la base de données
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Modifications enregistrées pour {positions.Count} positions");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors du traitement du message pour la caméra {message.CameraCode}");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service consommateur RabbitMQ arrêté");
        
        _channel?.Close();
        _connection?.Close();
        
        return base.StopAsync(cancellationToken);
    }
    
    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}