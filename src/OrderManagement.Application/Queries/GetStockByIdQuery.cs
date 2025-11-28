using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetStockByIdQuery : IRequest<StockDto?>
{
    public int Id { get; set; }
}



