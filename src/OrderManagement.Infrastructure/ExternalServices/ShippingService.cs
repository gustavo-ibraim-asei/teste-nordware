using Microsoft.Extensions.Logging;

namespace OrderManagement.Infrastructure.ExternalServices;

public class ShippingService
{
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(ILogger<ShippingService> logger)
    {
        _logger = logger;
    }

    public async Task<decimal> CalculateShippingAsync(string zipCode, decimal totalWeight, CancellationToken cancellationToken = default)
    {
        // Mock implementation - in production, this would call a real shipping API
        await Task.Delay(100, cancellationToken); // Simulate API call

        _logger.LogInformation("Calculando frete para CEP: {ZipCode}, peso: {Weight}", zipCode, totalWeight);

        // Simple mock calculation based on zip code region
        decimal baseCost = 10.00m;
        decimal regionMultiplier = GetRegionMultiplier(zipCode);
        decimal weightMultiplier = totalWeight > 1 ? 1.5m : 1.0m;

        return baseCost * regionMultiplier * weightMultiplier;
    }

    private decimal GetRegionMultiplier(string zipCode)
    {
        // Mock logic - different regions have different shipping costs
        if (zipCode.StartsWith("01") || zipCode.StartsWith("02") || zipCode.StartsWith("03"))
            return 1.0m; // São Paulo capital
        if (zipCode.StartsWith("0"))
            return 1.2m; // São Paulo state
        if (zipCode.StartsWith("1") || zipCode.StartsWith("2"))
            return 1.5m; // Other states
        return 2.0m; // Remote areas
    }
}

