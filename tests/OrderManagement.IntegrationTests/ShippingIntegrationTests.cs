using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.DTOs.ValueObjects;
using OrderManagement.IntegrationTests.Helpers;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class ShippingIntegrationTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;

    public ShippingIntegrationTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CalculateShipping_ForSaoPauloCapital_ShouldReturnMultipleOptions()
    {
        // Arrange
        ShippingCalculationRequestDto request = new ShippingCalculationRequestDto
        {
            ZipCode = "01310-100",
            OrderTotal = 100.00m,
            Weight = 1.0m
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shipping/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ShippingCalculationResponseDto? result = await response.Content.ReadFromJsonAsync<ShippingCalculationResponseDto>();
        result.Should().NotBeNull();
        result!.Options.Should().NotBeEmpty();
        result.Options.Should().HaveCountGreaterThan(1);
        
        // Should include same-day option for São Paulo capital
        result.Options.Should().Contain(o => o.IsSameDay == true);
    }

    [Fact]
    public async Task CalculateShipping_ForOrderAbove200_ShouldIncludeFreeShipping()
    {
        // Arrange
        ShippingCalculationRequestDto request = new ShippingCalculationRequestDto
        {
            ZipCode = "01310-100",
            OrderTotal = 250.00m,
            Weight = 1.0m
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shipping/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ShippingCalculationResponseDto? result = await response.Content.ReadFromJsonAsync<ShippingCalculationResponseDto>();
        result.Should().NotBeNull();
        result!.Options.Should().NotBeEmpty();
        
        ShippingOptionDto? freeOption = result.Options.FirstOrDefault(o => o.IsFree);
        freeOption.Should().NotBeNull();
        freeOption!.Price.Should().Be(0);
        freeOption.ShippingType.Should().Be("Padrão");
    }

    [Fact]
    public async Task CalculateShipping_ForRemoteArea_ShouldNotIncludeSameDay()
    {
        // Arrange
        ShippingCalculationRequestDto request = new ShippingCalculationRequestDto
        {
            ZipCode = "90000-000",
            OrderTotal = 100.00m,
            Weight = 1.0m
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shipping/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ShippingCalculationResponseDto? result = await response.Content.ReadFromJsonAsync<ShippingCalculationResponseDto>();
        result.Should().NotBeNull();
        result!.Options.Should().NotBeEmpty();
        
        result.Options.Should().NotContain(o => o.IsSameDay == true);
    }

    [Fact]
    public async Task CalculateShipping_ShouldReturnValidCarrierAndShippingTypeIds()
    {
        // Arrange
        ShippingCalculationRequestDto request = new ShippingCalculationRequestDto
        {
            ZipCode = "01310-100",
            OrderTotal = 100.00m,
            Weight = 1.0m
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shipping/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ShippingCalculationResponseDto? result = await response.Content.ReadFromJsonAsync<ShippingCalculationResponseDto>();
        result.Should().NotBeNull();
        
        foreach (ShippingOptionDto option in result!.Options)
        {
            option.CarrierId.Should().BeGreaterThan(0);
            option.ShippingTypeId.Should().BeGreaterThan(0);
            option.CarrierName.Should().NotBeNullOrEmpty();
            option.ShippingType.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task CalculateShipping_ShouldOrderOptionsByPrice()
    {
        // Arrange
        ShippingCalculationRequestDto request = new ShippingCalculationRequestDto
        {
            ZipCode = "01310-100",
            OrderTotal = 100.00m,
            Weight = 1.0m
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shipping/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ShippingCalculationResponseDto? result = await response.Content.ReadFromJsonAsync<ShippingCalculationResponseDto>();
        result.Should().NotBeNull();
        
        List<ShippingOptionDto> paidOptions = result!.Options.Where(o => !o.IsFree).ToList();
        if (paidOptions.Count > 1)
        {
            for (int i = 0; i < paidOptions.Count - 1; i++)
            {
                paidOptions[i].Price.Should().BeLessThanOrEqualTo(paidOptions[i + 1].Price);
            }
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

