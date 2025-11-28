using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public CreateOrderDto Order { get; set; } = null!;
}


