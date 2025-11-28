using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreatePriceTableCommand : IRequest<PriceTableDto>
{
    public CreatePriceTableDto PriceTable { get; set; } = null!;
}

