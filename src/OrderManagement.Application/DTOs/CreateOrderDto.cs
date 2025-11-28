using OrderManagement.Application.DTOs.ValueObjects;

namespace OrderManagement.Application.DTOs;

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public AddressDto ShippingAddress { get; set; } = null!;
    public int? SelectedCarrierId { get; set; }
    public int? SelectedShippingTypeId { get; set; }
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int ColorId { get; set; }
    public int SizeId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Propriedades internas (não expostas na API)
    public int? SkuId { get; set; }
    public int? StockOfficeId { get; set; }
}

