namespace OrderManagement.Application.DTOs;

public class ProductPriceDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int PriceTableId { get; set; }
    public string PriceTableName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductPriceDto
{
    public int ProductId { get; set; }
    public int PriceTableId { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateProductPriceDto
{
    public decimal UnitPrice { get; set; }
}

