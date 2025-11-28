using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class ProductPriceRepository : Repository<ProductPrice>, IProductPriceRepository
{
    public ProductPriceRepository(OrderManagementDbContext context) : base(context)
    {
    }

    public override async Task<ProductPrice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pp => pp.Product)
            .Include(pp => pp.PriceTable)
            .FirstOrDefaultAsync(pp => pp.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<ProductPrice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pp => pp.Product)
            .Include(pp => pp.PriceTable)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductPrice?> GetByProductAndPriceTableAsync(int productId, int priceTableId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pp => pp.Product)
            .Include(pp => pp.PriceTable)
            .FirstOrDefaultAsync(pp => pp.ProductId == productId && pp.PriceTableId == priceTableId, cancellationToken);
    }

    public async Task<IEnumerable<ProductPrice>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pp => pp.Product)
            .Include(pp => pp.PriceTable)
            .Where(pp => pp.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductPrice>> GetByPriceTableIdAsync(int priceTableId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pp => pp.Product)
            .Include(pp => pp.PriceTable)
            .Where(pp => pp.PriceTableId == priceTableId)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal?> GetPriceForProductAsync(int productId, int priceTableId, CancellationToken cancellationToken = default)
    {
        ProductPrice? productPrice = await GetByProductAndPriceTableAsync(productId, priceTableId, cancellationToken);
        return productPrice?.UnitPrice;
    }

    public override async Task<ProductPrice> AddAsync(ProductPrice entity, CancellationToken cancellationToken = default)
    {
        // Anexar as entidades relacionadas como Unchanged para evitar inserção
        if (entity.Product != null)
        {
            _context.Entry(entity.Product).State = EntityState.Unchanged;
        }
        if (entity.PriceTable != null)
        {
            _context.Entry(entity.PriceTable).State = EntityState.Unchanged;
        }
        
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }
}

