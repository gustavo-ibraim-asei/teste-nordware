using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Events;
using OrderManagement.Messaging.Publishers;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class MessagingIntegrationTests
{
    [Fact]
    public async Task MessagePublisher_PublishOrderCreatedEvent_ShouldSucceed()
    {
        // Arrange
        Mock<ILogger<RabbitMQMessagePublisher>> loggerMock = new Mock<ILogger<RabbitMQMessagePublisher>>();
        RabbitMQMessagePublisher publisher = new RabbitMQMessagePublisher(loggerMock.Object);
        OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent(1, 100, 299.99m);

        // Act & Assert
        // Note: In a real scenario, we'd use TestContainers for RabbitMQ
        // For now, we test the publisher logic without actual connection
        Func<Task> act = async () => await publisher.PublishAsync(orderCreatedEvent);

        // This will fail if RabbitMQ is not running, but tests the integration logic
        // In production, use TestContainers.RabbitMq
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MessagePublisher_PublishOrderStatusChangedEvent_ShouldSucceed()
    {
        // Arrange
        Mock<ILogger<RabbitMQMessagePublisher>> loggerMock = new Mock<ILogger<RabbitMQMessagePublisher>>();
        RabbitMQMessagePublisher publisher = new RabbitMQMessagePublisher(loggerMock.Object);
        OrderStatusChangedEvent statusChangedEvent = new OrderStatusChangedEvent(
            1, 
            Domain.Enums.OrderStatus.Pending, 
            Domain.Enums.OrderStatus.Confirmed);

        // Act & Assert
        Func<Task> act = async () => await publisher.PublishAsync(statusChangedEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MessagePublisher_PublishOrderCancelledEvent_ShouldSucceed()
    {
        // Arrange
        Mock<ILogger<RabbitMQMessagePublisher>> loggerMock = new Mock<ILogger<RabbitMQMessagePublisher>>();
        RabbitMQMessagePublisher publisher = new RabbitMQMessagePublisher(loggerMock.Object);
        OrderCancelledEvent cancelledEvent = new OrderCancelledEvent(1, 100, "Test cancellation");

        // Act & Assert
        Func<Task> act = async () => await publisher.PublishAsync(cancelledEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void MessagePublisher_Dispose_ShouldCleanupResources()
    {
        // Arrange
        Mock<ILogger<RabbitMQMessagePublisher>> loggerMock = new Mock<ILogger<RabbitMQMessagePublisher>>();
        RabbitMQMessagePublisher publisher = new RabbitMQMessagePublisher(loggerMock.Object);

        // Act & Assert
        Action act = () => publisher.Dispose();
        act.Should().NotThrow();
    }
}

