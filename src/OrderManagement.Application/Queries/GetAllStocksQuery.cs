using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllStocksQuery : IRequest<List<StockDto>>
{
    public int? SkuId { get; set; }
    public int? StockOfficeId { get; set; }
}



