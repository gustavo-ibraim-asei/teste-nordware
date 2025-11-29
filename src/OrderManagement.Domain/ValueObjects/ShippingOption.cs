namespace OrderManagement.Domain.ValueObjects;

public class ShippingOption
{
    public int CarrierId { get; private set; }
    public string CarrierName { get; private set; } = string.Empty;
    public int ShippingTypeId { get; private set; }
    public string ShippingType { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int EstimatedDays { get; private set; }
    public bool IsFree { get; private set; }
    public bool IsSameDay { get; private set; }

    private ShippingOption() { } // EF Core

    public ShippingOption(int carrierId, string carrierName, int shippingTypeId, string shippingType, decimal price, int estimatedDays, bool isFree = false, bool isSameDay = false)
    {
        if (carrierId <= 0)
            throw new ArgumentException("O ID da transportadora deve ser maior que zero", nameof(carrierId));

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("O nome da transportadora não pode ser vazio", nameof(carrierName));

        if (shippingTypeId <= 0)
            throw new ArgumentException("O ID do tipo de frete deve ser maior que zero", nameof(shippingTypeId));

        if (string.IsNullOrWhiteSpace(shippingType))
            throw new ArgumentException("O tipo de frete não pode ser vazio", nameof(shippingType));

        if (price < 0)
            throw new ArgumentException("O preço não pode ser negativo", nameof(price));

        if (estimatedDays < 0)
            throw new ArgumentException("Os dias estimados não podem ser negativos", nameof(estimatedDays));

        CarrierId = carrierId;
        CarrierName = carrierName;
        ShippingTypeId = shippingTypeId;
        ShippingType = shippingType;
        Price = price;
        EstimatedDays = estimatedDays;
        IsFree = isFree;
        IsSameDay = isSameDay;
    }
}

