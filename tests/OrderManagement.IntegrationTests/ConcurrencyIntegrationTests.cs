using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Multitenancy;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.IntegrationTests.Fixtures;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class ConcurrencyIntegrationTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly OrderManagementDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public ConcurrencyIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddScoped<ITenantProvider, TenantProvider>()
            .AddScoped<IOrderRepository>(sp => new OrderRepository(_dbContext))
            .AddScoped<IUserRepository>(sp => new UserRepository(_dbContext))
            .AddScoped<IUnitOfWork>(sp => new UnitOfWork(_dbContext, 
                sp.GetRequiredService<IOrderRepository>(), 
                sp.GetRequiredService<IUserRepository>()))
            .BuildServiceProvider();

        _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        _tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
        _tenantProvider.SetTenant("test-tenant");
    }

    [Fact]
    public async Task UpdateOrder_WithConcurrentModifications_ShouldHandleOptimisticConcurrency()
    {
        // Arrange - Create order
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 1, 29.99m)
        };
        Order order = new Order(1, address, items, "test-tenant");

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Act - Simulate concurrent updates
        Order? order1 = await _unitOfWork.Orders.GetByIdAsync(order.Id);
        Order? order2 = await _dbContext.Orders.FindAsync(order.Id);

        order1.Should().NotBeNull();
        order2.Should().NotBeNull();

        // First update
        order1!.UpdateStatus(OrderStatus.Confirmed);
        await _unitOfWork.SaveChangesAsync();

        // Second update (should detect concurrency conflict)
        order2!.UpdateStatus(OrderStatus.Shipped);
        
        // Modify RowVersion to simulate concurrent modification
        byte[] modifiedRowVersion = new byte[order2.RowVersion.Length];
        Array.Copy(order2.RowVersion, modifiedRowVersion, order2.RowVersion.Length);
        if (modifiedRowVersion.Length > 0)
        {
            modifiedRowVersion[0] = (byte)(modifiedRowVersion[0] + 1);
        }
        order2.GetType().GetProperty("RowVersion")?.SetValue(order2, modifiedRowVersion);

        // Assert - Should throw concurrency exception
        Func<Task> act = async () => await _unitOfWork.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("O pedido foi modificado por outro processo. Por favor, atualize e tente novamente.");
    }

    [Fact]
    public async Task UpdateOrder_WithSameRowVersion_ShouldSucceed()
    {
        // Arrange - Create order
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 1, 29.99m)
        };
        Order order = new Order(1, address, items, "test-tenant");

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Act - Update order normally
        Order? orderToUpdate = await _unitOfWork.Orders.GetByIdAsync(order.Id);
        orderToUpdate.Should().NotBeNull();
        orderToUpdate!.UpdateStatus(OrderStatus.Confirmed);
        
        int result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        
        Order? updatedOrder = await _dbContext.Orders.FindAsync(order.Id);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task BatchProcess_WithMultipleOrders_ShouldProcessInParallel()
    {
        // Arrange - Create multiple orders
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        
        List<Order> orders = new List<Order>();
        for (int i = 0; i < 5; i++)
        {
            Order order = new Order(i + 1, address, new List<OrderItem> { new OrderItem(1, "Product 1", 1, 10m) }, "test-tenant");
            orders.Add(order);
            await _unitOfWork.Orders.AddAsync(order);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act - Process all orders in parallel
        List<Task> tasks = orders.Select(async o =>
        {
            Order? order = await _unitOfWork.Orders.GetByIdAsync(o.Id);
            if (order != null)
            {
                order.UpdateStatus(OrderStatus.Confirmed);
                await _unitOfWork.SaveChangesAsync();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        // Assert - All orders should be confirmed
        foreach (Order originalOrder in orders)
        {
            Order? updatedOrder = await _dbContext.Orders.FindAsync(originalOrder.Id);
            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);
        }
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
    }
}





