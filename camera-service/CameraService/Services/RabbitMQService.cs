// Service producteur (CameraService)
using System.Text;
using System.Text.Json;
using CameraService.Messages;
using RabbitMQ.Client;

namespace CameraService.Services;
public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection _connection;
    private IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private const string ExchangeName = "camera_events";
    private const string QueueName = "camera_status_changes";
    private const string RoutingKey = "camera.status.changed";

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        
        try 
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
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
    }

    public Task PublishCameraStatusChangeAsync(string cameraCode, bool estAfficher)
    {
        try
        {
            var message = new CameraStatusChangedMessage
            {
                CameraCode = cameraCode,
                EstAfficher = estAfficher,
                Timestamp = DateTime.UtcNow
            };
            
            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);
            
            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: null,
                body: body);
            
            _logger.LogInformation($"Message publié: Caméra {cameraCode} est {(estAfficher ? "affichée" : "masquée")}");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la publication du message pour la caméra {cameraCode}");
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

public interface IRabbitMQService
{
    Task PublishCameraStatusChangeAsync(string cameraCode, bool estAfficher);
}