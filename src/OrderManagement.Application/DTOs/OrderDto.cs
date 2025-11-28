using OrderManagement.Application.DTOs.ValueObjects;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AddressDto ShippingAddress { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public string? TrackingNumber { get; set; }
    public int? CarrierId { get; set; }
    public string? CarrierName { get; set; }
    public int? ShippingTypeId { get; set; }
    public string? ShippingType { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? SkuId { get; set; }
    public int? StockOfficeId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

