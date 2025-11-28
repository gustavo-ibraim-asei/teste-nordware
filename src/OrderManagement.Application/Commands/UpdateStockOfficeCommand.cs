using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateStockOfficeCommand : IRequest<StockOfficeDto>
{
    public int Id { get; set; }
    public UpdateStockOfficeDto StockOffice { get; set; } = null!;
}



