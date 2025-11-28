using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Services;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.UnitTests.Application.Services;

public class ShippingCalculationServiceTests
{
    private readonly Mock<ILogger<ShippingCalculationService>> _loggerMock;
    private readonly ShippingCalculationService _service;

    public ShippingCalculationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ShippingCalculationService>>();
        _service = new ShippingCalculationService(_loggerMock.Object);
    }

    [Fact]
    public async Task CalculateShippingOptions_WithOrderAbove200_ShouldIncludeFreeShipping()
    {
        // Arrange
        string zipCode = "01310-100";
        decimal orderTotal = 250.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        ShippingOption? freeShippingOption = options.FirstOrDefault(o => o.IsFree);
        freeShippingOption.Should().NotBeNull();
        freeShippingOption!.Price.Should().Be(0);
        freeShippingOption.ShippingType.Should().Be("Padrão");
    }

    [Fact]
    public async Task CalculateShippingOptions_WithOrderBelow200_ShouldNotIncludeFreeShipping()
    {
        // Arrange
        string zipCode = "01310-100";
        decimal orderTotal = 150.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        ShippingOption? standardOption = options.FirstOrDefault(o => o.ShippingType == "Padrão");
        standardOption.Should().NotBeNull();
        standardOption!.IsFree.Should().BeFalse();
        standardOption.Price.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalculateShippingOptions_ForSaoPauloCapital_ShouldIncludeSameDayOption()
    {
        // Arrange
        string zipCode = "01310-100"; // São Paulo capital
        decimal orderTotal = 100.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        ShippingOption? sameDayOption = options.FirstOrDefault(o => o.IsSameDay);
        sameDayOption.Should().NotBeNull();
        sameDayOption!.ShippingType.Should().Be("Imediato");
        sameDayOption.CarrierName.Should().Be("Loggi");
    }

    [Fact]
    public async Task CalculateShippingOptions_ForRemoteArea_ShouldNotIncludeSameDayOption()
    {
        // Arrange
        string zipCode = "90000-000"; // Remote area
        decimal orderTotal = 100.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        ShippingOption? sameDayOption = options.FirstOrDefault(o => o.IsSameDay);
        sameDayOption.Should().BeNull();
    }

    [Fact]
    public async Task CalculateShippingOptions_ShouldReturnMultipleCarriers()
    {
        // Arrange
        string zipCode = "20000-000"; // Rio de Janeiro
        decimal orderTotal = 100.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        options.Should().HaveCountGreaterThan(1);
        List<string> carrierNames = options.Select(o => o.CarrierName).Distinct().ToList();
        carrierNames.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task CalculateShippingOptions_ShouldHaveValidCarrierAndShippingTypeIds()
    {
        // Arrange
        string zipCode = "01310-100";
        decimal orderTotal = 100.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        foreach (ShippingOption option in options)
        {
            option.CarrierId.Should().BeGreaterThan(0);
            option.ShippingTypeId.Should().BeGreaterThan(0);
            option.CarrierName.Should().NotBeNullOrEmpty();
            option.ShippingType.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task CalculateShippingOptions_ShouldOrderByPrice()
    {
        // Arrange
        string zipCode = "01310-100";
        decimal orderTotal = 100.00m;
        decimal totalWeight = 1.0m;

        // Act
        List<ShippingOption> options = await _service.CalculateShippingOptionsAsync(zipCode, orderTotal, totalWeight);

        // Assert
        options.Should().NotBeEmpty();
        // Free shipping should be first
        if (options.Any(o => o.IsFree))
        {
            options.First().IsFree.Should().BeTrue();
        }
        
        // Then ordered by price
        List<ShippingOption> paidOptions = options.Where(o => !o.IsFree).ToList();
        if (paidOptions.Count > 1)
        {
            for (int i = 0; i < paidOptions.Count - 1; i++)
            {
                paidOptions[i].Price.Should().BeLessThanOrEqualTo(paidOptions[i + 1].Price);
            }
        }
    }
}





