using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

public interface ISkuRepository : IRepository<Sku>
{
    /// <summary>
    /// Busca um SKU pela combinação de ProductId, ColorId e SizeId
    /// </summary>
    Task<Sku?> GetByProductColorSizeAsync(int productId, int colorId, int sizeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca SKUs por ProductId
    /// </summary>
    Task<IEnumerable<Sku>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca SKU por código
    /// </summary>
    Task<Sku?> GetBySkuCodeAsync(string skuCode, CancellationToken cancellationToken = default);
}



