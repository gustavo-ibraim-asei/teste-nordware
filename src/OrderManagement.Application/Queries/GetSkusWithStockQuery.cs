using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetSkusWithStockQuery : IRequest<List<SkuWithStockDto>>
{
}



