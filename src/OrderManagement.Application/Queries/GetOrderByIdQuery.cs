using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public int OrderId { get; set; }
}


