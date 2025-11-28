using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateStockCommand : IRequest<StockDto>
{
    public int Id { get; set; }
    public UpdateStockDto Stock { get; set; } = null!;
}



