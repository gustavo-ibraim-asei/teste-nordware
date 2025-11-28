using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

