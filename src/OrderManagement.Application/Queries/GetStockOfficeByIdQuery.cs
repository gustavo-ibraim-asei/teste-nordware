using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetStockOfficeByIdQuery : IRequest<StockOfficeDto?>
{
    public int Id { get; set; }
}



