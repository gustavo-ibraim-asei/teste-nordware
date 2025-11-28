using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CompleteOrderCommand : IRequest<OrderDto>
{
    public int OrderId { get; set; }
    public CompleteOrderDto ShippingInfo { get; set; } = null!;
}





