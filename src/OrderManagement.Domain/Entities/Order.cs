using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities;

public class Order : BaseEntity
{
    public int CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public decimal TotalAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public string? TrackingNumber { get; private set; }
    public int? CarrierId { get; private set; }
    public string? CarrierName { get; private set; }
    public int? ShippingTypeId { get; private set; }
    public string? ShippingType { get; private set; }
    public int EstimatedDeliveryDays { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;

    // Navigation properties
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    private Order() { } // EF Core

    public Order(int customerId, Address shippingAddress, List<OrderItem> items, string tenantId)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Order must have at least one item", nameof(items));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        CustomerId = customerId;
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Items = items;
        TenantId = tenantId;

        CalculateTotal();

        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId, TotalAmount));
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot update status of a cancelled order");

        if (Status == OrderStatus.Delivered && newStatus != OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot change status of a delivered order");

        OrderStatus oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order");

        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel a shipped order. Contact support.");

        OrderStatus oldStatus = Status;
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason));
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Can only add items to pending orders");

        Items.Add(item);
        CalculateTotal();
    }

    public void UpdateShippingCost(decimal shippingCost)
    {
        if (shippingCost < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(shippingCost));

        ShippingCost = shippingCost;
        CalculateTotal();
    }

    public void SetShippingInfo(int carrierId, string carrierName, int shippingTypeId, string shippingType, decimal shippingCost, int estimatedDays)
    {
        if (carrierId <= 0)
            throw new ArgumentException("Carrier ID must be greater than zero", nameof(carrierId));

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name cannot be empty", nameof(carrierName));

        if (shippingTypeId <= 0)
            throw new ArgumentException("Shipping type ID must be greater than zero", nameof(shippingTypeId));

        if (string.IsNullOrWhiteSpace(shippingType))
            throw new ArgumentException("Shipping type cannot be empty", nameof(shippingType));

        if (shippingCost < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(shippingCost));

        if (estimatedDays < 0)
            throw new ArgumentException("Estimated days cannot be negative", nameof(estimatedDays));

        CarrierId = carrierId;
        CarrierName = carrierName;
        ShippingTypeId = shippingTypeId;
        ShippingType = shippingType;
        ShippingCost = shippingCost;
        EstimatedDeliveryDays = estimatedDays;
        CalculateTotal();
    }

    public void SetTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number cannot be empty", nameof(trackingNumber));

        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Subtotal) + ShippingCost;
    }
}

