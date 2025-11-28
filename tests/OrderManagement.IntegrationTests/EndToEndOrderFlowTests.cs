using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.DTOs.ValueObjects;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Multitenancy;
using OrderManagement.IntegrationTests.Helpers;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class EndToEndOrderFlowTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly OrderManagementDbContext _dbContext;

    public EndToEndOrderFlowTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        
        ITenantProvider tenantProvider = _scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenant("e2e-tenant");
    }

    [Fact]
    public async Task EndToEnd_CreateOrder_UpdateStatus_Cancel_ShouldWork()
    {
        // Step 1: Create Order
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
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        OrderDto? createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();
        createdOrder.Should().NotBeNull();
        createdOrder!.Status.Should().Be(OrderStatus.Pending);

        // Step 2: Verify in Database
        Domain.Entities.Order? orderInDb = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == createdOrder.Id);
        
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Pending);
        orderInDb.Items.Should().HaveCount(1);

        // Step 3: Update Status to Confirmed
        UpdateOrderStatusDto updateStatusDto = new UpdateOrderStatusDto { Status = OrderStatus.Confirmed };
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder!.Id}/status", 
            updateStatusDto);
        
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? updatedOrder = await updateResponse.Content.ReadFromJsonAsync<OrderDto>();
        updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);

        // Step 4: Verify Status in Database
        Domain.Entities.Order? updatedOrderInDb = await _dbContext.Orders.FindAsync(createdOrder.Id);
        updatedOrderInDb!.Status.Should().Be(OrderStatus.Confirmed);

        // Step 5: Cancel Order
        HttpResponseMessage cancelResponse = await _client.DeleteAsync($"/api/orders/{createdOrder.Id}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDto? cancelledOrder = await cancelResponse.Content.ReadFromJsonAsync<OrderDto>();
        cancelledOrder!.Status.Should().Be(OrderStatus.Cancelled);

        // Step 6: Verify Cancellation in Database
        Domain.Entities.Order? cancelledOrderInDb = await _dbContext.Orders.FindAsync(createdOrder.Id);
        cancelledOrderInDb!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task EndToEnd_ListOrders_WithFilters_ShouldWork()
    {
        // Arrange - Create multiple orders
        AddressDto address = new AddressDto
        {
            Street = "Rua Teste",
            Number = "123",
            Neighborhood = "Centro",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01310-100"
        };

        CreateOrderDto order1 = new CreateOrderDto
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductId = 1, ProductName = "P1", Quantity = 1, UnitPrice = 10m } },
            ShippingAddress = address
        };

        CreateOrderDto order2 = new CreateOrderDto
        {
            CustomerId = 2,
            Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductId = 2, ProductName = "P2", Quantity = 1, UnitPrice = 20m } },
            ShippingAddress = address
        };

        await _client.PostAsJsonAsync("/api/orders", order1);
        HttpResponseMessage response2 = await _client.PostAsJsonAsync("/api/orders", order2);
        OrderDto? createdOrder2 = await response2.Content.ReadFromJsonAsync<OrderDto>();
        
        // Update order2 status
        await _client.PutAsJsonAsync(
            $"/api/orders/{createdOrder2!.Id}/status",
            new UpdateOrderStatusDto { Status = OrderStatus.Confirmed });

        // Act - Filter by Customer
        HttpResponseMessage customer1Orders = await _client.GetAsync("/api/orders?customerId=1");
        customer1Orders.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResultDto<OrderDto>? customer1Result = await customer1Orders.Content.ReadFromJsonAsync<PagedResultDto<OrderDto>>();
        customer1Result!.Items.Should().Contain(o => o.CustomerId == 1);

        // Act - Filter by Status
        HttpResponseMessage confirmedOrders = await _client.GetAsync("/api/orders?status=2"); // Confirmed = 2
        confirmedOrders.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResultDto<OrderDto>? confirmedResult = await confirmedOrders.Content.ReadFromJsonAsync<PagedResultDto<OrderDto>>();
        confirmedResult!.Items.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Confirmed));
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
    }
}

