using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class StockRepository : Repository<Stock>, IStockRepository
{
    public StockRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    public override async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.StockOffice)
            .Include(s => s.Sku)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.StockOffice)
            .Include(s => s.Sku)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetBySkuAsync(int skuId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.StockOffice)
            .Include(s => s.Sku)
            .Where(s => s.SkuId == skuId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Stock?> GetAvailableStockAsync(int skuId, int quantity, CancellationToken cancellationToken = default)
    {
        // Busca a primeira filial que tem estoque disponível suficiente
        // Ordena por quantidade disponível (maior primeiro) para otimizar distribuição
        return await _dbSet
            .Include(s => s.StockOffice)
            .Include(s => s.Sku)
            .Where(s => s.SkuId == skuId && (s.Quantity - s.Reserved) >= quantity)
            .OrderByDescending(s => s.Quantity - s.Reserved)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Stock?> GetBySkuAndOfficeAsync(int skuId, int stockOfficeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.StockOffice)
            .Include(s => s.Sku)
            .FirstOrDefaultAsync(s => s.SkuId == skuId && s.StockOfficeId == stockOfficeId, cancellationToken);
    }

    public async Task<IEnumerable<Stock>> GetByStockOfficeAsync(int stockOfficeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.StockOffice)
            .Include(s => s.Sku)
            .ThenInclude(sku => sku!.Color)
            .Include(s => s.Sku)
            .ThenInclude(sku => sku!.Size)
            .Where(s => s.StockOfficeId == stockOfficeId)
            .ToListAsync(cancellationToken);
    }
}



