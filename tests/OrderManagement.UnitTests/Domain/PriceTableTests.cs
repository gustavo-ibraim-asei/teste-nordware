using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class PriceTableTests
{
    private const string TenantId = "tenant1";

    [Fact]
    public void Constructor_WithValidData_ShouldCreatePriceTable()
    {
        // Arrange & Act
        PriceTable priceTable = new PriceTable("Tabela Padrão", "Tabela de preços padrão", TenantId);

        // Assert
        priceTable.Name.Should().Be("Tabela Padrão");
        priceTable.Description.Should().Be("Tabela de preços padrão");
        priceTable.IsActive.Should().BeTrue();
        priceTable.TenantId.Should().Be(TenantId);
        priceTable.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new PriceTable("", "Descrição", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new PriceTable("Tabela", "Descrição", "");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithNameExceeding200Characters_ShouldThrowArgumentException()
    {
        // Arrange
        string longName = new string('A', 201);

        // Act & Assert
        Action act = () => new PriceTable(longName, "Descrição", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode exceder 200 caracteres*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela", "Descrição", TenantId);
        var originalUpdatedAt = priceTable.UpdatedAt;

        // Act
        priceTable.UpdateName("Tabela Atualizada");

        // Assert
        priceTable.Name.Should().Be("Tabela Atualizada");
        priceTable.UpdatedAt.Should().NotBeNull();
        priceTable.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
    }

    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela", "Descrição", TenantId);

        // Act & Assert
        Action act = () => priceTable.UpdateName("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode ser vazio*");
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela", "Descrição", TenantId);

        // Act
        priceTable.UpdateDescription("Nova descrição");

        // Assert
        priceTable.Description.Should().Be("Nova descrição");
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldSetDescriptionToNull()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela", "Descrição", TenantId);

        // Act
        priceTable.UpdateDescription(null);

        // Assert
        priceTable.Description.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela", "Descrição", TenantId);
        priceTable.Deactivate();

        // Act
        priceTable.Activate();

        // Assert
        priceTable.IsActive.Should().BeTrue();
        priceTable.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela", "Descrição", TenantId);

        // Act
        priceTable.Deactivate();

        // Assert
        priceTable.IsActive.Should().BeFalse();
        priceTable.UpdatedAt.Should().NotBeNull();
    }
}

