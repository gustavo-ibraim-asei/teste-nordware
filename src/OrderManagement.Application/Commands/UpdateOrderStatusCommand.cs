using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Commands;

public class UpdateOrderStatusCommand : IRequest<OrderDto>
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
}


