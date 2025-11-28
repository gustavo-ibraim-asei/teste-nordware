using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class SizeTests
{
    private const string TenantId = "tenant1";

    [Fact]
    public void Constructor_WithValidData_ShouldCreateSize()
    {
        // Arrange & Act
        Size size = new Size("Médio", "M", TenantId);

        // Assert
        size.Name.Should().Be("Médio");
        size.Code.Should().Be("M");
        size.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Size("", "M", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Size("Médio", "M", "");
        act.Should().Throw<ArgumentException>()
            .WithMessage("TenantId cannot be empty*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        Size size = new Size("Médio", "M", TenantId);

        // Act
        size.UpdateName("Médio Grande");

        // Assert
        size.Name.Should().Be("Médio Grande");
    }

    [Fact]
    public void UpdateCode_WithValidCode_ShouldUpdateCode()
    {
        // Arrange
        Size size = new Size("Médio", "M", TenantId);

        // Act
        size.UpdateCode("MG");

        // Assert
        size.Code.Should().Be("MG");
    }
}


