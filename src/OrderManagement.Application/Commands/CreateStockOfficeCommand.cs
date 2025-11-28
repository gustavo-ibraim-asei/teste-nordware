using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateStockOfficeCommand : IRequest<StockOfficeDto>
{
    public CreateStockOfficeDto StockOffice { get; set; } = null!;
}



