using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CancelOrderCommand : IRequest<OrderDto>
{
    public int OrderId { get; set; }
    public string? Reason { get; set; }
}



