namespace OrderManagement.Application.DTOs;

public class StockOfficeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStockOfficeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class UpdateStockOfficeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}



