using Microsoft.Extensions.Logging;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public class ShippingCalculationService : IShippingCalculationService
{
    private readonly ILogger<ShippingCalculationService> _logger;

    public ShippingCalculationService(ILogger<ShippingCalculationService> logger)
    {
        _logger = logger;
    }

    public async Task<List<ShippingOption>> CalculateShippingOptionsAsync(string zipCode, decimal orderTotal, decimal totalWeight, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculando opções de frete para CEP: {ZipCode}, valor do pedido: {OrderTotal}, peso: {Weight}", zipCode, orderTotal, totalWeight);

        List<ShippingOption> options = new List<ShippingOption>();

        // Calcular base cost baseado na região
        decimal baseCost = CalculateBaseCost(zipCode, totalWeight);
        decimal regionMultiplier = GetRegionMultiplier(zipCode);
        decimal weightMultiplier = totalWeight > 1 ? 1.5m : 1.0m;
        decimal standardShippingCost = baseCost * regionMultiplier * weightMultiplier;

        // Regra: Pedido acima de R$ 200 tem frete padrão grátis
        bool hasFreeShipping = orderTotal >= 200.00m;

        // IDs das transportadoras (mockados)
        const int CARRIER_CORREIOS = 1;
        const int CARRIER_LOGGI = 2;
        const int CARRIER_JADLOG = 3;

        // IDs dos tipos de frete (mockados)
        const int TYPE_PADRAO = 1;
        const int TYPE_EXPRESSO = 2;
        const int TYPE_IMEDIATO = 3;
        const int TYPE_ECONOMICO = 4;

        // Opção 1: Frete Padrão (2 dias úteis)
        decimal standardPrice = hasFreeShipping ? 0 : standardShippingCost;
        options.Add(new ShippingOption(CARRIER_CORREIOS, "Correios", TYPE_PADRAO, "Padrão", standardPrice, 2, hasFreeShipping));

        // Opção 2: Frete Expresso (1 dia útil)
        decimal expressCost = standardShippingCost * 1.5m;
        options.Add(new ShippingOption(CARRIER_CORREIOS, "Correios", TYPE_EXPRESSO, "Expresso", expressCost, 1, false));

        // Opção 3: Frete Imediato (mesmo dia) - apenas para grandes centros
        if (IsSameDayAvailable(zipCode))
        {
            decimal immediateCost = standardShippingCost * 3.0m;
            options.Add(new ShippingOption(CARRIER_LOGGI, "Loggi", TYPE_IMEDIATO, "Imediato", immediateCost, 0, false, true));
        }

        // Opção 4: Transportadora Alternativa (para regiões específicas)
        if (ShouldOfferAlternativeCarrier(zipCode))
        {
            decimal alternativeCost = standardShippingCost * 0.9m; // 10% mais barato
            options.Add(new ShippingOption(CARRIER_JADLOG, "JadLog", TYPE_ECONOMICO, "Econômico", alternativeCost, 3, false));
        }

        // Ordenar por preço (frete grátis primeiro)
        options = options.OrderBy(o => o.IsFree ? 0 : 1)
                        .ThenBy(o => o.Price)
                        .ThenBy(o => o.EstimatedDays)
                        .ToList();

        _logger.LogInformation("Calculadas {Count} opções de frete para CEP {ZipCode}", options.Count, zipCode);

        return await Task.FromResult(options);
    }

    private decimal CalculateBaseCost(string zipCode, decimal totalWeight)
    {
        // Custo base variável por região
        decimal baseCost = zipCode.StartsWith("01") || zipCode.StartsWith("02") || zipCode.StartsWith("03")
            ? 10.00m  // São Paulo capital
            : zipCode.StartsWith("0")
            ? 12.00m  // São Paulo estado
            : zipCode.StartsWith("1") || zipCode.StartsWith("2")
            ? 15.00m  // Outros estados
            : 20.00m; // Regiões remotas

        // Adicionar custo por peso
        decimal weightCost = totalWeight > 1 ? (totalWeight - 1) * 2.00m : 0;
        
        return baseCost + weightCost;
    }

    private decimal GetRegionMultiplier(string zipCode)
    {
        if (zipCode.StartsWith("01") || zipCode.StartsWith("02") || zipCode.StartsWith("03"))
            return 1.0m; // São Paulo capital
        if (zipCode.StartsWith("0"))
            return 1.2m; // São Paulo state
        if (zipCode.StartsWith("1") || zipCode.StartsWith("2"))
            return 1.5m; // Other states
        return 2.0m; // Remote areas
    }

    private bool IsSameDayAvailable(string zipCode)
    {
        // Entrega imediata disponível apenas em grandes centros
        return zipCode.StartsWith("01") || zipCode.StartsWith("02") || zipCode.StartsWith("03") ||
               zipCode.StartsWith("20") || zipCode.StartsWith("21") || zipCode.StartsWith("22") || // Rio de Janeiro
               zipCode.StartsWith("30") || zipCode.StartsWith("31") || zipCode.StartsWith("32"); // Belo Horizonte
    }

    private bool ShouldOfferAlternativeCarrier(string zipCode)
    {
        // Oferecer transportadora alternativa para regiões específicas
        return zipCode.StartsWith("1") || zipCode.StartsWith("2") || zipCode.StartsWith("3");
    }
}

