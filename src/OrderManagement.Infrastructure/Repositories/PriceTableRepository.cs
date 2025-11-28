using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class PriceTableRepository : Repository<PriceTable>, IPriceTableRepository
{
    public PriceTableRepository(OrderManagementDbContext context) : base(context)
    {
    }

    public async Task<PriceTable?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pt => pt.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<PriceTable>> GetActivePriceTablesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pt => pt.IsActive)
            .ToListAsync(cancellationToken);
    }
}

