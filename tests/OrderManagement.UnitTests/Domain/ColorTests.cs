using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class ColorTests
{
    private const string TenantId = "tenant1";

    [Fact]
    public void Constructor_WithValidData_ShouldCreateColor()
    {
        // Arrange & Act
        Color color = new Color("Preto", "BLK", TenantId);

        // Assert
        color.Name.Should().Be("Preto");
        color.Code.Should().Be("BLK");
        color.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Color("", "BLK", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Color("Preto", "BLK", "");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId não pode ser vazio*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        Color color = new Color("Preto", "BLK", TenantId);

        // Act
        color.UpdateName("Preto Escuro");

        // Assert
        color.Name.Should().Be("Preto Escuro");
    }

    [Fact]
    public void UpdateCode_WithValidCode_ShouldUpdateCode()
    {
        // Arrange
        Color color = new Color("Preto", "BLK", TenantId);

        // Act
        color.UpdateCode("BLK2");

        // Assert
        color.Code.Should().Be("BLK2");
    }
}


