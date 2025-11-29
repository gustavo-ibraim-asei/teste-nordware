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

public class OrderCreationFlowTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly OrderManagementDbContext _dbContext;

    public OrderCreationFlowTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        
        // Set tenant
        ITenantProvider tenantProvider = _scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenant("test-tenant");
    }

    [Fact]
    public async Task CreateOrder_CompleteFlow_ShouldSucceed()
    {
        // Arrange
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
                },
                new CreateOrderItemDto
                {
                    ProductId = 2,
                    ProductName = "Product 2",
                    Quantity = 1,
                    UnitPrice = 49.99m
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

        // Act - Create Order
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/orders", createOrderDto);
        
        // Assert - Creation
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();
        createdOrder.Should().NotBeNull();
        createdOrder!.Id.Should().BeGreaterThan(0);
        createdOrder.Status.Should().Be(OrderStatus.Pending);
        createdOrder.Items.Should().HaveCount(2);
        createdOrder.TotalAmount.Should().Be(109.97m); // (2 * 29.99) + (1 * 49.99)

        // Act - Get Order by ID
        HttpResponseMessage getResponse = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");
        
        // Assert - Retrieval
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? retrievedOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Id.Should().Be(createdOrder.Id);
        retrievedOrder.Items.Should().HaveCount(2);

        // Act - Update Status
        UpdateOrderStatusDto updateStatusDto = new UpdateOrderStatusDto { Status = OrderStatus.Confirmed };
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder.Id}/status", 
            updateStatusDto);

        // Assert - Status Update
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? updatedOrder = await updateResponse.Content.ReadFromJsonAsync<OrderDto>();
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);

        // Verify in database
        Domain.Entities.Order? orderInDb = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == createdOrder.Id);
        
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Confirmed);
        orderInDb.Items.Should().HaveCount(2);
        orderInDb.TenantId.Should().Be("test-tenant");
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        CreateOrderDto invalidOrderDto = new CreateOrderDto
        {
            CustomerId = 0, // Invalid
            Items = new List<CreateOrderItemDto>(), // Empty
            ShippingAddress = new AddressDto
            {
                Street = "", // Invalid
                Number = "",
                Neighborhood = "",
                City = "",
                State = "",
                ZipCode = ""
            }
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/orders", invalidOrderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_ThenCancel_ShouldSucceed()
    {
        // Arrange
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

        // Act - Create
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/orders", createOrderDto);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act - Cancel
        HttpResponseMessage cancelResponse = await _client.DeleteAsync($"/api/orders/{createdOrder!.Id}?reason=Test cancellation");

        // Assert
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? cancelledOrder = await cancelResponse.Content.ReadFromJsonAsync<OrderDto>();
        cancelledOrder.Should().NotBeNull();
        cancelledOrder!.Status.Should().Be(OrderStatus.Cancelled);

        // Verify in database
        Domain.Entities.Order? orderInDb = await _dbContext.Orders.FindAsync(createdOrder.Id);
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Cancelled);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
    }
}

