using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.IntegrationTests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public OrderManagementDbContext DbContext { get; private set; }

    public DatabaseFixture()
    {
        DbContextOptions<OrderManagementDbContext> options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new OrderManagementDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}

