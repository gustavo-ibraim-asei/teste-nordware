using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdatePriceTableCommand : IRequest<PriceTableDto>
{
    public int Id { get; set; }
    public UpdatePriceTableDto PriceTable { get; set; } = null!;
}

