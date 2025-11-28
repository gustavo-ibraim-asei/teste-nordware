using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class DecreaseStockCommand : IRequest<StockDto>
{
    public int StockId { get; set; }
    public DecreaseStockDto Decrease { get; set; } = null!;
}



