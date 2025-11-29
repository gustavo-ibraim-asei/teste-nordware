using FluentAssertions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        int customerId = 1;
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 2, 29.99m),
            new OrderItem(2, "Product 2", 1, 49.99m)
        };

        // Act
        Order order = new Order(customerId, address, items, "test-tenant");

        // Assert
        order.Should().NotBeNull();
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().HaveCount(2);
        order.TotalAmount.Should().Be(109.97m); // (2 * 29.99) + (1 * 49.99) + 0 shipping
    }

    [Fact]
    public void CreateOrder_WithEmptyItems_ShouldThrowException()
    {
        // Arrange
        int customerId = 1;
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>();

        // Act & Assert
        Func<Order> act = () => new Order(customerId, address, items, "test-tenant");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O pedido deve ter pelo menos um item*");
    }

    [Fact]
    public void UpdateStatus_FromPendingToConfirmed_ShouldUpdateSuccessfully()
    {
        // Arrange
        Order order = CreateValidOrder();

        // Act
        order.UpdateStatus(OrderStatus.Confirmed);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStatus_FromDelivered_ShouldThrowException()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.UpdateStatus(OrderStatus.Delivered);

        // Act & Assert
        Func<object> act = () => { order.UpdateStatus(OrderStatus.Confirmed); return null; };
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Não é possível alterar o status de um pedido entregue*");
    }

    [Fact]
    public void Cancel_WhenPending_ShouldCancelSuccessfully()
    {
        // Arrange
        Order order = CreateValidOrder();

        // Act
        order.Cancel("Customer request");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_WhenDelivered_ShouldThrowException()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.UpdateStatus(OrderStatus.Delivered);

        // Act & Assert
        Func<object> act = () => { order.Cancel(); return null; };
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Não é possível cancelar um pedido entregue*");
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrowException()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.UpdateStatus(OrderStatus.Shipped);

        // Act & Assert
        Func<object> act = () => { order.Cancel(); return null; };
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Não é possível cancelar um pedido enviado*");
    }

    [Fact]
    public void UpdateShippingCost_ShouldRecalculateTotal()
    {
        // Arrange
        Order order = CreateValidOrder();
        decimal shippingCost = 15.00m;

        // Act
        order.UpdateShippingCost(shippingCost);

        // Assert
        order.ShippingCost.Should().Be(shippingCost);
        order.TotalAmount.Should().Be(109.97m + shippingCost);
    }

    private Order CreateValidOrder()
    {
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 2, 29.99m),
            new OrderItem(2, "Product 2", 1, 49.99m)
        };
        return new Order(1, address, items, "test-tenant");
    }
}

