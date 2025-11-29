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

public class DatabaseIntegrationTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly OrderManagementDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public DatabaseIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;

        // Create repositories for all required UnitOfWork constructor parameters
        var orderRepository = new OrderRepository(_dbContext);
        var userRepository = new UserRepository(_dbContext);
        var skuRepository = new SkuRepository(_dbContext);
        var stockRepository = new StockRepository(_dbContext);
        var productRepository = new Repository<Product>(_dbContext);
        var stockOfficeRepository = new Repository<StockOffice>(_dbContext);
        var colorRepository = new Repository<Color>(_dbContext);
        var sizeRepository = new Repository<Size>(_dbContext);
        var priceTableRepository = new PriceTableRepository(_dbContext);
        var productPriceRepository = new ProductPriceRepository(_dbContext);
        var customerRepository = new CustomerRepository(_dbContext);

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddScoped<ITenantProvider, TenantProvider>()
            .AddScoped<IUnitOfWork>(sp => new UnitOfWork(
                _dbContext,
                orderRepository,
                userRepository,
                skuRepository,
                stockRepository,
                productRepository,
                stockOfficeRepository,
                colorRepository,
                sizeRepository,
                priceTableRepository,
                productPriceRepository,
                customerRepository
            ))
            .AddScoped<IOrderRepository>(sp => orderRepository)
            .BuildServiceProvider();

        _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        _tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
        _tenantProvider.SetTenant("test-tenant");
    }

    [Fact]
    public async Task Repository_AddOrder_ShouldPersistToDatabase()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 2, 29.99m),
            new OrderItem(2, "Product 2", 1, 49.99m)
        };
        Order order = new Order(1, address, items, "test-tenant");

        // Act
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        Order? orderInDb = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        orderInDb.Should().NotBeNull();
        orderInDb!.CustomerId.Should().Be(1);
        orderInDb.Status.Should().Be(OrderStatus.Pending);
        orderInDb.Items.Should().HaveCount(2);
        orderInDb.TenantId.Should().Be("test-tenant");
    }

    [Fact]
    public async Task Repository_GetByCustomerId_ShouldReturnFilteredResults()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        
        Order order1 = new Order(1, address, new List<OrderItem> { new OrderItem(1, "Product 1", 1, 10m) }, "test-tenant");
        Order order2 = new Order(2, address, new List<OrderItem> { new OrderItem(2, "Product 2", 1, 20m) }, "test-tenant");
        Order order3 = new Order(1, address, new List<OrderItem> { new OrderItem(3, "Product 3", 1, 30m) }, "test-tenant");

        await _unitOfWork.Orders.AddAsync(order1);
        await _unitOfWork.Orders.AddAsync(order2);
        await _unitOfWork.Orders.AddAsync(order3);
        await _unitOfWork.SaveChangesAsync();

        // Act
        IEnumerable<Order> customerOrders = await _unitOfWork.Orders.GetByCustomerIdAsync(1);

        // Assert
        customerOrders.Should().HaveCount(2);
        customerOrders.All(o => o.CustomerId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task Repository_GetByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        
        Order order1 = new Order(1, address, new List<OrderItem> { new OrderItem(1, "Product 1", 1, 10m) }, "test-tenant");
        Order order2 = new Order(2, address, new List<OrderItem> { new OrderItem(2, "Product 2", 1, 20m) }, "test-tenant");
        
        order1.UpdateStatus(OrderStatus.Confirmed);

        await _unitOfWork.Orders.AddAsync(order1);
        await _unitOfWork.Orders.AddAsync(order2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        IEnumerable<Order> confirmedOrders = await _unitOfWork.Orders.GetByStatusAsync(OrderStatus.Confirmed);
        IEnumerable<Order> pendingOrders = await _unitOfWork.Orders.GetByStatusAsync(OrderStatus.Pending);

        // Assert
        confirmedOrders.Should().HaveCount(1);
        confirmedOrders.First().Status.Should().Be(OrderStatus.Confirmed);
        
        pendingOrders.Should().HaveCount(1);
        pendingOrders.First().Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task UnitOfWork_Transaction_RollbackOnError()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        Order order = new Order(1, address, new List<OrderItem> { new OrderItem(1, "Product 1", 1, 10m) }, "test-tenant");

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        Order? orderInDb = await _dbContext.Orders.FindAsync(order.Id);
        orderInDb.Should().BeNull(); // Should not be persisted after rollback
    }

    [Fact]
    public async Task UnitOfWork_Transaction_CommitSuccessfully()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        Order order = new Order(1, address, new List<OrderItem> { new OrderItem(1, "Product 1", 1, 10m) }, "test-tenant");

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        Order? orderInDb = await _dbContext.Orders.FindAsync(order.Id);
        orderInDb.Should().NotBeNull(); // Should be persisted after commit
    }

    [Fact]
    public async Task Multitenancy_FilterByTenant_ShouldIsolateData()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        
        Order tenant1Order = new Order(1, address, new List<OrderItem> { new OrderItem(1, "Product 1", 1, 10m) }, "tenant-1");
        Order tenant2Order = new Order(1, address, new List<OrderItem> { new OrderItem(2, "Product 2", 1, 20m) }, "tenant-2");

        await _unitOfWork.Orders.AddAsync(tenant1Order);
        await _unitOfWork.Orders.AddAsync(tenant2Order);
        await _unitOfWork.SaveChangesAsync();

        // Act - Set tenant 1
        _tenantProvider.SetTenant("tenant-1");
        IEnumerable<Order> tenant1Orders = await _unitOfWork.Orders.GetAllAsync();

        // Assert
        tenant1Orders.Should().HaveCount(1);
        tenant1Orders.First().TenantId.Should().Be("tenant-1");
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
    }
}

