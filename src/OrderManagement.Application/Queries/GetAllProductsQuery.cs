using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllProductsQuery : IRequest<List<ProductDto>>
{
}



