using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderManagement.Domain.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderManagement.Messaging.Consumers;

public class OrderStatusChangedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;
    private const string QueueName = "order_status_changed_inventory";
    private const string ExchangeName = "order_events";
    private const string RoutingKey = "order.status.changed";
    private const string DlqExchangeName = "order_events_dlx";
    private const string DlqQueueName = "order_status_changed_inventory_dlq";

    public OrderStatusChangedConsumer(ILogger<OrderStatusChangedConsumer> logger)
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

        // Declare queue with DLQ
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DlqExchangeName },
                { "x-dead-letter-routing-key", "order.status.changed.failed" }
            });

        // Declare DLQ exchange
        _channel.ExchangeDeclare(
            exchange: DlqExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        // Declare DLQ
        _channel.QueueDeclare(
            queue: DlqQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind DLQ to DLQ exchange
        _channel.QueueBind(
            queue: DlqQueueName,
            exchange: DlqExchangeName,
            routingKey: "order.status.changed.failed");

        // Bind main queue to exchange
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

            try
            {
                OrderStatusChangedEvent? statusChangedEvent = JsonConvert.DeserializeObject<OrderStatusChangedEvent>(message);
                if (statusChangedEvent != null)
                {
                    await ProcessStatusChangedAsync(statusChangedEvent, stoppingToken);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento de mudança de status do pedido. Mensagem: {Message}", message);
                _channel.BasicNack(ea.DeliveryTag, false, false); // Send to DLQ
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("OrderStatusChangedConsumer iniciado. Aguardando mensagens...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessStatusChangedAsync(OrderStatusChangedEvent statusChangedEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando evento de mudança de status do pedido. OrderId: {OrderId}, Status Anterior: {OldStatus}, Novo Status: {NewStatus}",
            statusChangedEvent.OrderId,
            statusChangedEvent.OldStatus,
            statusChangedEvent.NewStatus);

        // Simulate inventory update
        if (statusChangedEvent.NewStatus == Domain.Enums.OrderStatus.Confirmed)
        {
            await Task.Delay(100, cancellationToken);
            _logger.LogInformation("Estoque atualizado para pedido {OrderId}", statusChangedEvent.OrderId);
        }
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

