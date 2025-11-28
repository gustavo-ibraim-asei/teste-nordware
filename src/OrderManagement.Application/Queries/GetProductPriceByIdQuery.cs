using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetProductPriceByIdQuery : IRequest<ProductPriceDto?>
{
    public int Id { get; set; }
}

