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

public class PaymentWebhookIntegrationTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly OrderManagementDbContext _dbContext;

    public PaymentWebhookIntegrationTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        
        ITenantProvider tenantProvider = _scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenant("test-tenant");
    }

    [Fact]
    public async Task PaymentWebhook_WithPaidStatus_ShouldUpdateOrderToConfirmed()
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

        // Act - Send payment webhook
        PaymentWebhookDto webhookDto = new PaymentWebhookDto
        {
            OrderId = createdOrder!.Id,
            PaymentStatus = "Paid",
            TransactionId = "TXN-12345"
        };

        HttpResponseMessage webhookResponse = await _client.PostAsJsonAsync("/api/payment/payment-update", webhookDto);

        // Assert
        webhookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify order status updated
        Domain.Entities.Order? orderInDb = await _dbContext.Orders.FindAsync(createdOrder.Id);
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task PaymentWebhook_WithFailedStatus_ShouldUpdateOrderToCancelled()
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

        // Act - Send payment webhook with failed status
        PaymentWebhookDto webhookDto = new PaymentWebhookDto
        {
            OrderId = createdOrder!.Id,
            PaymentStatus = "Failed",
            TransactionId = "TXN-12345"
        };

        HttpResponseMessage webhookResponse = await _client.PostAsJsonAsync("/api/payment/payment-update", webhookDto);

        // Assert
        webhookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify order status updated
        Domain.Entities.Order? orderInDb = await _dbContext.Orders.FindAsync(createdOrder.Id);
        orderInDb.Should().NotBeNull();
        orderInDb!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task PaymentWebhook_WithNonExistentOrder_ShouldReturnNotFound()
    {
        // Act - Send webhook for non-existent order
        PaymentWebhookDto webhookDto = new PaymentWebhookDto
        {
            OrderId = 99999,
            PaymentStatus = "Paid",
            TransactionId = "TXN-12345"
        };

        HttpResponseMessage webhookResponse = await _client.PostAsJsonAsync("/api/payment/payment-update", webhookDto);

        // Assert
        webhookResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
    }
}

