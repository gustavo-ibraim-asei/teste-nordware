using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class StockOfficeTests
{
    private const string TenantId = "tenant1";

    [Fact]
    public void Constructor_WithValidData_ShouldCreateStockOffice()
    {
        // Arrange & Act
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);

        // Assert
        stockOffice.Name.Should().Be("Filial SP");
        stockOffice.Code.Should().Be("SP01");
        stockOffice.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new StockOffice("", "SP01", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new StockOffice("Filial SP", "SP01", "");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId não pode ser vazio*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);

        // Act
        stockOffice.UpdateName("Filial São Paulo");

        // Assert
        stockOffice.Name.Should().Be("Filial São Paulo");
    }

    [Fact]
    public void UpdateCode_WithValidCode_ShouldUpdateCode()
    {
        // Arrange
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);

        // Act
        stockOffice.UpdateCode("SP02");

        // Assert
        stockOffice.Code.Should().Be("SP02");
    }
}


