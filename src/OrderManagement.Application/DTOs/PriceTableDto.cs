namespace OrderManagement.Application.DTOs;

public class PriceTableDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePriceTableDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdatePriceTableDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

