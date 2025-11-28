using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

public interface IPriceTableRepository : IRepository<PriceTable>
{
    Task<PriceTable?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceTable>> GetActivePriceTablesAsync(CancellationToken cancellationToken = default);
}

