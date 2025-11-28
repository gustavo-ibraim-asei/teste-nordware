namespace OrderManagement.Application.DTOs;

public class SkuDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ColorId { get; set; }
    public int SizeId { get; set; }
    public string? Barcode { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ColorDto? Color { get; set; }
    public SizeDto? Size { get; set; }
}

public class CreateSkuDto
{
    public int ProductId { get; set; }
    public int ColorId { get; set; }
    public int SizeId { get; set; }
}

public class UpdateSkuDto
{
    public string? Barcode { get; set; }
}

public class SkuWithStockDto
{
    public SkuDto Sku { get; set; } = null!;
    public int TotalAvailableQuantity { get; set; }
    public List<StockInfoDto> Stocks { get; set; } = new();
}

public class StockInfoDto
{
    public int StockOfficeId { get; set; }
    public string StockOfficeName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
}

