using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}


