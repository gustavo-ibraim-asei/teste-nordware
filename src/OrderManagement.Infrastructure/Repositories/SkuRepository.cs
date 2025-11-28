using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class SkuRepository : Repository<Sku>, ISkuRepository
{
    public SkuRepository(OrderManagementDbContext context) : base(context)
    {
    }

    public override async Task<Sku?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.Color)
            .Include(s => s.Size)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Sku>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.Color)
            .Include(s => s.Size)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sku?> GetByProductColorSizeAsync(int productId, int colorId, int sizeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.Color)
            .Include(s => s.Size)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.ColorId == colorId && s.SizeId == sizeId, cancellationToken);
    }

    public async Task<IEnumerable<Sku>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.Color)
            .Include(s => s.Size)
            .Where(s => s.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sku?> GetBySkuCodeAsync(string skuCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Product)
            .Include(s => s.Color)
            .Include(s => s.Size)
            .FirstOrDefaultAsync(s => s.SkuCode == skuCode, cancellationToken);
    }

    public override async Task<Sku> AddAsync(Sku entity, CancellationToken cancellationToken = default)
    {
        // Anexar as entidades relacionadas como Unchanged para evitar inserção
        // Isso garante que apenas o SKU será inserido, não as entidades relacionadas
        if (entity.Product != null)
        {
            _context.Entry(entity.Product).State = EntityState.Unchanged;
        }
        if (entity.Color != null)
        {
            _context.Entry(entity.Color).State = EntityState.Unchanged;
        }
        if (entity.Size != null)
        {
            _context.Entry(entity.Size).State = EntityState.Unchanged;
        }
        
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }
}
