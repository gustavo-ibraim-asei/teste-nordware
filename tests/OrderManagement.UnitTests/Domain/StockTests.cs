using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class StockTests
{
    private const string TenantId = "tenant1";

    [Fact]
    public void Constructor_WithValidData_ShouldCreateStock()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);

        // Act
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);

        // Assert
        stock.SkuId.Should().Be(sku.Id);
        stock.StockOfficeId.Should().Be(stockOffice.Id);
        stock.Quantity.Should().Be(100);
        stock.Reserved.Should().Be(0);
        stock.AvailableQuantity.Should().Be(100);
        stock.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);

        // Act & Assert
        Action act = () => new Stock(sku.Id, stockOffice.Id, -1, TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity cannot be negative*");
    }

    [Fact]
    public void Reserve_WithValidQuantity_ShouldReserveStock()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);

        // Act
        stock.Reserve(30);

        // Assert
        stock.Reserved.Should().Be(30);
        stock.AvailableQuantity.Should().Be(70);
    }

    [Fact]
    public void Reserve_WithQuantityExceedingAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);

        // Act & Assert
        Action act = () => stock.Reserve(150);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Estoque insuficiente*");
    }

    [Fact]
    public void Decrease_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);
        stock.Reserve(30);

        // Act
        stock.Decrease(30);

        // Assert
        stock.Quantity.Should().Be(70);
        stock.Reserved.Should().Be(0);
        stock.AvailableQuantity.Should().Be(70);
    }

    [Fact]
    public void Decrease_WithQuantityExceedingAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);

        // Act & Assert
        Action act = () => stock.Decrease(150);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Estoque insuficiente*");
    }

    [Fact]
    public void Increase_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);

        // Act
        stock.Increase(50);

        // Assert
        stock.Quantity.Should().Be(150);
        stock.AvailableQuantity.Should().Be(150);
    }

    [Fact]
    public void ReleaseReservation_WithValidQuantity_ShouldReleaseReservation()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        Stock stock = new Stock(sku.Id, stockOffice.Id, 100, TenantId);
        stock.Reserve(30);

        // Act
        stock.ReleaseReservation(30);

        // Assert
        stock.Reserved.Should().Be(0);
        stock.AvailableQuantity.Should().Be(100);
    }
}


