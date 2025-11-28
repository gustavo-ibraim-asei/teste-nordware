using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    ISkuRepository Skus { get; }
    IStockRepository Stocks { get; }
    IRepository<Product> Products { get; }
    IRepository<StockOffice> StockOffices { get; }
    IRepository<Color> Colors { get; }
    IRepository<Size> Sizes { get; }
    IPriceTableRepository PriceTables { get; }
    IProductPriceRepository ProductPrices { get; }
    ICustomerRepository Customers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

