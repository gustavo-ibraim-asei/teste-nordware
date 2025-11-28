using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateStockCommand : IRequest<StockDto>
{
    public CreateStockDto Stock { get; set; } = null!;
}



