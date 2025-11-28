using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllProductPricesQuery : IRequest<List<ProductPriceDto>>
{
    public int? ProductId { get; set; }
    public int? PriceTableId { get; set; }
}

