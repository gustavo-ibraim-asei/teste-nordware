using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Events;
using RabbitMQ.Client;

namespace OrderManagement.Messaging.Publishers;

public class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessagePublisher> _logger;
    private const string ExchangeName = "order_events";

    public RabbitMQMessagePublisher(ILogger<RabbitMQMessagePublisher> logger)
    {
        _logger = logger;

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            string eventType = domainEvent.GetType().Name;
            string routingKey = GetRoutingKey(domainEvent);
            string message = JsonConvert.SerializeObject(domainEvent);
            byte[] body = Encoding.UTF8.GetBytes(message);

            IBasicProperties properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = eventType;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Evento {EventType} publicado com routing key {RoutingKey}", eventType, routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento {EventType}", domainEvent.GetType().Name);
            throw;
        }
    }

    private string GetRoutingKey(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            Domain.Events.OrderCreatedEvent => "order.created",
            Domain.Events.OrderStatusChangedEvent => "order.status.changed",
            Domain.Events.OrderCancelledEvent => "order.cancelled",
            _ => "order.unknown"
        };
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}

