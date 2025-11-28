using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class OrderItemTests
{
    [Fact]
    public void CreateOrderItem_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        OrderItem item = new OrderItem(1, "Product 1", 2, 29.99m);

        // Assert
        item.Should().NotBeNull();
        item.ProductId.Should().Be(1);
        item.ProductName.Should().Be("Product 1");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(29.99m);
        item.Subtotal.Should().Be(59.98m);
    }

    [Fact]
    public void CreateOrderItem_WithInvalidProductId_ShouldThrowException()
    {
        // Act & Assert
        Func<OrderItem> act = () => new OrderItem(0, "Product 1", 1, 29.99m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Product ID must be greater than zero*");
    }

    [Fact]
    public void CreateOrderItem_WithEmptyProductName_ShouldThrowException()
    {
        // Act & Assert
        Func<OrderItem> act = () => new OrderItem(1, "", 1, 29.99m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Product name cannot be empty*");
    }

    [Fact]
    public void CreateOrderItem_WithInvalidQuantity_ShouldThrowException()
    {
        // Act & Assert
        Func<OrderItem> act = () => new OrderItem(1, "Product 1", 0, 29.99m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*");
    }

    [Fact]
    public void UpdateQuantity_ShouldRecalculateSubtotal()
    {
        // Arrange
        OrderItem item = new OrderItem(1, "Product 1", 2, 29.99m);

        // Act
        item.UpdateQuantity(5);

        // Assert
        item.Quantity.Should().Be(5);
        item.Subtotal.Should().Be(149.95m);
    }
}

