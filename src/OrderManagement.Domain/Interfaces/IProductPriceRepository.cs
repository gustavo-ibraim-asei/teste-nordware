using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

public interface IProductPriceRepository : IRepository<ProductPrice>
{
    Task<ProductPrice?> GetByProductAndPriceTableAsync(int productId, int priceTableId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPrice>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPrice>> GetByPriceTableIdAsync(int priceTableId, CancellationToken cancellationToken = default);
    Task<decimal?> GetPriceForProductAsync(int productId, int priceTableId, CancellationToken cancellationToken = default);
}

