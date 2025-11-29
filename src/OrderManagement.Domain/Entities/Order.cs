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
            throw new ArgumentException("O pedido deve ter pelo menos um item", nameof(items));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId não pode ser vazio", nameof(tenantId));

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
            throw new InvalidOperationException("Não é possível atualizar o status de um pedido cancelado");

        if (Status == OrderStatus.Delivered && newStatus != OrderStatus.Cancelled)
            throw new InvalidOperationException("Não é possível alterar o status de um pedido entregue");

        OrderStatus oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Não é possível cancelar um pedido entregue");

        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Não é possível cancelar um pedido enviado. Entre em contato com o suporte.");

        OrderStatus oldStatus = Status;
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason));
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só é possível adicionar itens a pedidos pendentes");

        Items.Add(item);
        CalculateTotal();
    }

    public void UpdateShippingCost(decimal shippingCost)
    {
        if (shippingCost < 0)
            throw new ArgumentException("O custo de frete não pode ser negativo", nameof(shippingCost));

        ShippingCost = shippingCost;
        CalculateTotal();
    }

    public void SetShippingInfo(int carrierId, string carrierName, int shippingTypeId, string shippingType, decimal shippingCost, int estimatedDays)
    {
        if (carrierId <= 0)
            throw new ArgumentException("O ID da transportadora deve ser maior que zero", nameof(carrierId));

        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("O nome da transportadora não pode ser vazio", nameof(carrierName));

        if (shippingTypeId <= 0)
            throw new ArgumentException("O ID do tipo de frete deve ser maior que zero", nameof(shippingTypeId));

        if (string.IsNullOrWhiteSpace(shippingType))
            throw new ArgumentException("O tipo de frete não pode ser vazio", nameof(shippingType));

        if (shippingCost < 0)
            throw new ArgumentException("O custo de frete não pode ser negativo", nameof(shippingCost));

        if (estimatedDays < 0)
            throw new ArgumentException("Os dias estimados não podem ser negativos", nameof(estimatedDays));

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
            throw new ArgumentException("O número de rastreamento não pode ser vazio", nameof(trackingNumber));

        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Subtotal) + ShippingCost;
    }
}

