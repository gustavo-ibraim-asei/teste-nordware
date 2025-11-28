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
            throw new ArgumentException("Carrier ID must be greater than zero", nameof(carrierId));

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name cannot be empty", nameof(carrierName));

        if (shippingTypeId <= 0)
            throw new ArgumentException("Shipping type ID must be greater than zero", nameof(shippingTypeId));

        if (string.IsNullOrWhiteSpace(shippingType))
            throw new ArgumentException("Shipping type cannot be empty", nameof(shippingType));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (estimatedDays < 0)
            throw new ArgumentException("Estimated days cannot be negative", nameof(estimatedDays));

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

