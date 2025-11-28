using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetOrdersQuery : IRequest<PagedResultDto<OrderDto>>
{
    public OrderQueryDto Query { get; set; } = null!;
}


