using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetPriceTableByIdQuery : IRequest<PriceTableDto?>
{
    public int Id { get; set; }
}

