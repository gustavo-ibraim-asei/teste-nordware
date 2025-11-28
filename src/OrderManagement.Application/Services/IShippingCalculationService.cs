using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public interface IShippingCalculationService
{
    Task<List<ShippingOption>> CalculateShippingOptionsAsync(
        string zipCode,
        decimal orderTotal,
        decimal totalWeight,
        CancellationToken cancellationToken = default);
}


