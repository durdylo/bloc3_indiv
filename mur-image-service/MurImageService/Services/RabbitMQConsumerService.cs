// Service consommateur (MurImageService)
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MurImageService.Messages;
using MurImageService.Models;
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
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion à RabbitMQ");
            throw;
        }
        
        return base.StartAsync(cancellationToken);
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
                    var body = args.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<CameraStatusChangedMessage>(messageJson);
                    
                    if (message != null)
                    {
                        await HandleCameraStatusChanged(message);
                    }
                    
                    _channel.BasicAck(args.DeliveryTag, false);
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
                position.EstActif = message.EstAfficher;
                _logger.LogInformation($"Position {position.Id} du mur d'image {position.IdMurImage} mise à jour: EstActif = {position.EstActif}");
                positionsUpdated = true;
            }
            
            if (positionsUpdated)
            {
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