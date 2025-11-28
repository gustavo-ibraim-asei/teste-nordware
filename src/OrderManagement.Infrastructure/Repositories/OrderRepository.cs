using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}


