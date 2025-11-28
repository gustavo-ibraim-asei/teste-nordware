using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderManagement.Messaging.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private const string QueueName = "order_created_notifications";
    private const string ExchangeName = "order_events";
    private const string RoutingKey = "order.created";

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

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

        // Declare queue with DLQ
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "order_events_dlx" },
                { "x-dead-letter-routing-key", "order.created.failed" }
            });

        // Declare DLQ
        _channel.ExchangeDeclare(
            exchange: "order_events_dlx",
            type: ExchangeType.Topic,
            durable: true);

        _channel.QueueDeclare(
            queue: "order_created_notifications_dlq",
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            string routingKey = ea.RoutingKey;
            string messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();

            using IServiceScope scope = _serviceProvider.CreateScope();
            IdempotentMessageProcessor idempotentProcessor = scope.ServiceProvider.GetRequiredService<IdempotentMessageProcessor>();
            ITenantProvider tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
            string tenantId = tenantProvider.GetCurrentTenant();

            try
            {
                // Check idempotency
                if (await idempotentProcessor.IsMessageProcessedAsync(messageId, stoppingToken))
                {
                    _logger.LogWarning("Mensagem {MessageId} já processada. Ignorando.", messageId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                OrderCreatedEvent? orderCreatedEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(message);
                if (orderCreatedEvent != null)
                {
                    await ProcessOrderCreatedAsync(orderCreatedEvent, stoppingToken);

                    // Mark as processed
                    await idempotentProcessor.MarkAsProcessedAsync(messageId, nameof(OrderCreatedEvent), tenantId, stoppingToken);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento de criação de pedido. Mensagem: {Message}", message);
                _channel.BasicNack(ea.DeliveryTag, false, false); // Send to DLQ
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("OrderCreatedConsumer iniciado. Aguardando mensagens...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando evento de criação de pedido. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
            orderCreatedEvent.OrderId,
            orderCreatedEvent.CustomerId,
            orderCreatedEvent.TotalAmount);

        // Simulate email notification
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Notificação por email enviada para pedido {OrderId}", orderCreatedEvent.OrderId);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}

