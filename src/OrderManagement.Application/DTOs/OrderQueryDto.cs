using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public class OrderQueryDto
{
    public int? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}


