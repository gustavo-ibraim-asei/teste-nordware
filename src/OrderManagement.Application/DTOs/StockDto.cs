namespace OrderManagement.Application.DTOs;

public class StockDto
{
    public int Id { get; set; }
    public int SkuId { get; set; }
    public int StockOfficeId { get; set; }
    public int Quantity { get; set; }
    public int Reserved { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SkuDto? Sku { get; set; }
    public StockOfficeDto? StockOffice { get; set; }
}

public class CreateStockDto
{
    public int SkuId { get; set; }
    public int StockOfficeId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateStockDto
{
    public int Quantity { get; set; }
}

public class ReserveStockDto
{
    public int Quantity { get; set; }
}

public class DecreaseStockDto
{
    public int Quantity { get; set; }
}



