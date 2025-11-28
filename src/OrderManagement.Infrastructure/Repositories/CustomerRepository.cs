using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(OrderManagementDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Customer>()
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task<List<Customer>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Customer>()
            .Where(c => c.Name.Contains(name))
            .ToListAsync(cancellationToken);
    }
}

