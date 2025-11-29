using FluentAssertions;
using OrderManagement.Domain.Entities;
using Xunit;

namespace OrderManagement.UnitTests.Domain;

public class ProductTests
{
    private const string TenantId = "tenant1";

    [Fact]
    public void Constructor_WithValidData_ShouldCreateProduct()
    {
        // Arrange & Act
        Product product = new Product("Camiseta Básica", "CAM001", "Camiseta de algodão", TenantId);

        // Assert
        product.Name.Should().Be("Camiseta Básica");
        product.Code.Should().Be("CAM001");
        product.Description.Should().Be("Camiseta de algodão");
        product.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Product("", "CAM001", "Descrição", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithEmptyCode_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Product("Camiseta", "", "Descrição", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O código não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => new Product("Camiseta", "CAM001", "Descrição", "");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TenantId não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithNameExceeding200Characters_ShouldThrowArgumentException()
    {
        // Arrange
        string longName = new string('A', 201);

        // Act & Assert
        Action act = () => new Product(longName, "CAM001", "Descrição", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode exceder 200 caracteres*");
    }

    [Fact]
    public void Constructor_WithCodeExceeding50Characters_ShouldThrowArgumentException()
    {
        // Arrange
        string longCode = new string('A', 51);

        // Act & Assert
        Action act = () => new Product("Camiseta", longCode, "Descrição", TenantId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O código não pode exceder 50 caracteres*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);

        // Act
        product.UpdateName("Camiseta Atualizada");

        // Assert
        product.Name.Should().Be("Camiseta Atualizada");
    }

    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);

        // Act & Assert
        Action act = () => product.UpdateName("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*O nome não pode ser vazio*");
    }

    [Fact]
    public void UpdateCode_WithValidCode_ShouldUpdateCode()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);

        // Act
        product.UpdateCode("CAM002");

        // Assert
        product.Code.Should().Be("CAM002");
    }

    [Fact]
    public void UpdateCode_WithEmptyCode_ShouldThrowArgumentException()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);

        // Act & Assert
        Action act = () => product.UpdateCode("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("O código não pode ser vazio*");
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);

        // Act
        product.UpdateDescription("Nova descrição");

        // Assert
        product.Description.Should().Be("Nova descrição");
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldSetDescriptionToNull()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);

        // Act
        product.UpdateDescription(null);

        // Assert
        product.Description.Should().BeNull();
    }
}


