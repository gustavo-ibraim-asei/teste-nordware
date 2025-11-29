using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Application.Services;

public class StockServiceTests
{
    private readonly Mock<ISkuRepository> _skuRepositoryMock;
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<ILogger<StockService>> _loggerMock;
    private readonly StockService _service;
    private const string TenantId = "default";

    public StockServiceTests()
    {
        _skuRepositoryMock = new Mock<ISkuRepository>();
        _stockRepositoryMock = new Mock<IStockRepository>();
        _loggerMock = new Mock<ILogger<StockService>>();

        _service = new StockService(
            _skuRepositoryMock.Object,
            _stockRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CheckAvailabilityAsync_WithAvailableStock_ShouldReturnAvailability()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Stock stock = new Stock(1, 1, 100, TenantId);

        _skuRepositoryMock.Setup(r => r.GetByProductColorSizeAsync(1, 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        _stockRepositoryMock.Setup(r => r.GetAvailableStockAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        // Act
        StockAvailabilityResult? result = await _service.CheckAvailabilityAsync(1, 2, 3, 10, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.SkuId.Should().Be(sku.Id);
        result.StockOfficeId.Should().Be(stockOffice.Id);
        result.AvailableQuantity.Should().Be(100);
        result.StockOfficeName.Should().Be("Filial SP");
    }

    [Fact]
    public async Task CheckAvailabilityAsync_WithNonExistentSku_ShouldReturnNull()
    {
        // Arrange
        _skuRepositoryMock.Setup(r => r.GetByProductColorSizeAsync(1, 1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sku?)null);

        // Act
        StockAvailabilityResult? result = await _service.CheckAvailabilityAsync(1, 1, 1, 10, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckAvailabilityAsync_WithInsufficientStock_ShouldReturnNull()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);

        _skuRepositoryMock.Setup(r => r.GetByProductColorSizeAsync(1, 1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sku);

        _stockRepositoryMock.Setup(r => r.GetAvailableStockAsync(sku.Id, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);

        // Act
        StockAvailabilityResult? result = await _service.CheckAvailabilityAsync(1, 1, 1, 100, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DecreaseStockAsync_WithValidStock_ShouldDecreaseStock()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);
        Stock stock = new Stock(1, 1, 100, TenantId);

        _stockRepositoryMock.Setup(r => r.GetBySkuAndOfficeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        _stockRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DecreaseStockAsync(1, 1, 10, CancellationToken.None);

        // Assert
        stock.Quantity.Should().Be(90);
        _stockRepositoryMock.Verify(r => r.UpdateAsync(stock, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecreaseStockAsync_WithNonExistentStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _stockRepositoryMock.Setup(r => r.GetBySkuAndOfficeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);

        // Act & Assert
        Func<Task> act = async () => await _service.DecreaseStockAsync(1, 1, 10, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Estoque não encontrado para SKU 1 na filial 1");
    }
}


