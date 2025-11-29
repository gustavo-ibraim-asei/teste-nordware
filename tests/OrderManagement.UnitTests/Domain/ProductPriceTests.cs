using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class ProductPriceTests
{
    private const string TenantId = "tenant1";

    private Product CreateProduct()
    {
        return new Product("Produto Teste", "PROD001", "Descrição", TenantId);
    }

    private PriceTable CreatePriceTable()
    {
        return new PriceTable("Tabela Teste", "Descrição", TenantId);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateProductPrice()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = CreatePriceTable();
        decimal unitPrice = 99.99m;

        // Act
        ProductPrice productPrice = new ProductPrice(product, priceTable, unitPrice, TenantId);

        // Assert
        productPrice.ProductId.Should().Be(product.Id);
        productPrice.PriceTableId.Should().Be(priceTable.Id);
        productPrice.UnitPrice.Should().Be(unitPrice);
        productPrice.TenantId.Should().Be(TenantId);
        productPrice.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithNullProduct_ShouldThrowArgumentNullException()
    {
        // Arrange
        PriceTable priceTable = CreatePriceTable();

        // Act & Assert
        Action act = () => new ProductPrice(null!, priceTable, 99.99m, TenantId);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("product");
    }

    [Fact]
    public void Constructor_WithNullPriceTable_ShouldThrowArgumentNullException()
    {
        // Arrange
        Product product = CreateProduct();

        // Act & Assert
        Action act = () => new ProductPrice(product, null!, 99.99m, TenantId);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("priceTable");
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = CreatePriceTable();

        // Act & Assert
        Action act = () => new ProductPrice(product, priceTable, 99.99m, "");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = CreatePriceTable();

        // Act & Assert
        Action act = () => new ProductPrice(product, priceTable, -10m, TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O preço unitário não pode ser negativo*");
    }

    [Fact]
    public void Constructor_WithDifferentTenantProduct_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = new Product("Produto", "PROD001", null, "tenant2");
        PriceTable priceTable = CreatePriceTable();

        // Act & Assert
        Action act = () => new ProductPrice(product, priceTable, 99.99m, TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O produto deve pertencer ao mesmo tenant*");
    }

    [Fact]
    public void Constructor_WithDifferentTenantPriceTable_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = new PriceTable("Tabela", null, "tenant2");

        // Act & Assert
        Action act = () => new ProductPrice(product, priceTable, 99.99m, TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*A tabela de preços deve pertencer ao mesmo tenant*");
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = CreatePriceTable();
        ProductPrice productPrice = new ProductPrice(product, priceTable, 99.99m, TenantId);
        var originalUpdatedAt = productPrice.UpdatedAt;

        // Act
        productPrice.UpdatePrice(149.99m);

        // Assert
        productPrice.UnitPrice.Should().Be(149.99m);
        productPrice.UpdatedAt.Should().NotBeNull();
        productPrice.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = CreatePriceTable();
        ProductPrice productPrice = new ProductPrice(product, priceTable, 99.99m, TenantId);

        // Act & Assert
        Action act = () => productPrice.UpdatePrice(-10m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O preço unitário não pode ser negativo*");
    }

    [Fact]
    public void UpdatePrice_WithZeroPrice_ShouldUpdatePrice()
    {
        // Arrange
        Product product = CreateProduct();
        PriceTable priceTable = CreatePriceTable();
        ProductPrice productPrice = new ProductPrice(product, priceTable, 99.99m, TenantId);

        // Act
        productPrice.UpdatePrice(0m);

        // Assert
        productPrice.UnitPrice.Should().Be(0m);
    }
}

