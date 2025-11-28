using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class ReserveStockCommand : IRequest<StockDto>
{
    public int StockId { get; set; }
    public ReserveStockDto Reserve { get; set; } = null!;
}



