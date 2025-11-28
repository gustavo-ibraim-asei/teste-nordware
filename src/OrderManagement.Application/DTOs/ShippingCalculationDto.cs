namespace OrderManagement.Application.DTOs;

public class ShippingCalculationRequestDto
{
    public string ZipCode { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public decimal TotalWeight { get; set; }
}

public class ShippingCalculationResponseDto
{
    public List<ShippingOptionDto> Options { get; set; } = new();
    public string ZipCode { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
}

public class ShippingOptionDto
{
    public int CarrierId { get; set; }
    public string CarrierName { get; set; } = string.Empty;
    public int ShippingTypeId { get; set; }
    public string ShippingType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int EstimatedDays { get; set; }
    public bool IsFree { get; set; }
    public bool IsSameDay { get; set; }
    public string Description { get; set; } = string.Empty;
}

