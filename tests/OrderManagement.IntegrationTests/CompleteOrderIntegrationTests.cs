using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.DTOs.ValueObjects;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Multitenancy;
using OrderManagement.IntegrationTests.Helpers;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class CompleteOrderIntegrationTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly OrderManagementDbContext _dbContext;

    public CompleteOrderIntegrationTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        
        ITenantProvider tenantProvider = _scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenant("test-tenant");
    }

    [Fact]
    public async Task CompleteOrder_WithValidShippingInfo_ShouldCompleteSuccessfully()
    {
        // Arrange - Create order
        CreateOrderDto createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    Quantity = 2,
                    UnitPrice = 29.99m
                }
            },
            ShippingAddress = new AddressDto
            {
                Street = "Rua Teste",
                Number = "123",
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01310-100"
            }
        };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/orders", createOrderDto);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act - Complete order with shipping
        CompleteOrderDto completeOrderDto = new CompleteOrderDto
        {
            CarrierId = 1,
            ShippingTypeId = 1 // Padrão
        };

        HttpResponseMessage completeResponse = await _client.PostAsJsonAsync(
            $"/api/orders/{createdOrder!.Id}/complete",
            completeOrderDto);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? completedOrder = await completeResponse.Content.ReadFromJsonAsync<OrderDto>();
        completedOrder.Should().NotBeNull();
        completedOrder!.Status.Should().Be(OrderStatus.Confirmed);
        completedOrder.CarrierId.Should().Be(1);
        completedOrder.ShippingTypeId.Should().Be(1);
        completedOrder.ShippingCost.Should().BeGreaterThanOrEqualTo(0);

        // Verify in database
        Domain.Entities.Order? orderInDb = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == createdOrder.Id);
        
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Confirmed);
        orderInDb.CarrierId.Should().Be(1);
        orderInDb.ShippingTypeId.Should().Be(1);
    }

    [Fact]
    public async Task CompleteOrder_WithOrderAbove200_ShouldApplyFreeShipping()
    {
        // Arrange - Create order above R$ 200
        CreateOrderDto createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    Quantity = 10,
                    UnitPrice = 25.00m // Total: R$ 250
                }
            },
            ShippingAddress = new AddressDto
            {
                Street = "Rua Teste",
                Number = "123",
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01310-100"
            }
        };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/orders", createOrderDto);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act - Complete with standard shipping (should be free)
        CompleteOrderDto completeOrderDto = new CompleteOrderDto
        {
            CarrierId = 1,
            ShippingTypeId = 1 // Padrão
        };

        HttpResponseMessage completeResponse = await _client.PostAsJsonAsync(
            $"/api/orders/{createdOrder!.Id}/complete",
            completeOrderDto);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? completedOrder = await completeResponse.Content.ReadFromJsonAsync<OrderDto>();
        completedOrder.Should().NotBeNull();
        completedOrder!.ShippingCost.Should().Be(0);
    }

    [Fact]
    public async Task CompleteOrder_WithNonPendingOrder_ShouldReturnBadRequest()
    {
        // Arrange - Create and confirm order
        CreateOrderDto createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    Quantity = 1,
                    UnitPrice = 29.99m
                }
            },
            ShippingAddress = new AddressDto
            {
                Street = "Rua Teste",
                Number = "123",
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01310-100"
            }
        };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/orders", createOrderDto);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Update status to Confirmed
        await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder!.Id}/status",
            new UpdateOrderStatusDto { Status = OrderStatus.Confirmed });

        // Act - Try to complete already confirmed order
        CompleteOrderDto completeOrderDto = new CompleteOrderDto
        {
            CarrierId = 1,
            ShippingTypeId = 1
        };

        HttpResponseMessage completeResponse = await _client.PostAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/complete",
            completeOrderDto);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteOrder_WithInvalidShippingOption_ShouldReturnBadRequest()
    {
        // Arrange - Create order
        CreateOrderDto createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    Quantity = 1,
                    UnitPrice = 29.99m
                }
            },
            ShippingAddress = new AddressDto
            {
                Street = "Rua Teste",
                Number = "123",
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01310-100"
            }
        };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/orders", createOrderDto);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act - Try to complete with invalid shipping option
        CompleteOrderDto completeOrderDto = new CompleteOrderDto
        {
            CarrierId = 999, // Invalid
            ShippingTypeId = 999 // Invalid
        };

        HttpResponseMessage completeResponse = await _client.PostAsJsonAsync(
            $"/api/orders/{createdOrder!.Id}/complete",
            completeOrderDto);

        // Assert
        completeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
    }
}





