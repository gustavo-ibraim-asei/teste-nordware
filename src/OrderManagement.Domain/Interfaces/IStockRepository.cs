using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

public interface IStockRepository : IRepository<Stock>
{
    /// <summary>
    /// Busca estoque por SKU
    /// </summary>
    Task<IEnumerable<Stock>> GetBySkuAsync(int skuId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca estoque disponível (com quantidade suficiente) para um SKU
    /// Retorna a primeira filial que tem estoque suficiente
    /// </summary>
    Task<Stock?> GetAvailableStockAsync(int skuId, int quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca estoque por SKU e Filial
    /// </summary>
    Task<Stock?> GetBySkuAndOfficeAsync(int skuId, int stockOfficeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca todos os estoques de uma filial
    /// </summary>
    Task<IEnumerable<Stock>> GetByStockOfficeAsync(int stockOfficeId, CancellationToken cancellationToken = default);
}



