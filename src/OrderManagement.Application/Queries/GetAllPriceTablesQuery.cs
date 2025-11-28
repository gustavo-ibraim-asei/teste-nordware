using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllPriceTablesQuery : IRequest<List<PriceTableDto>>
{
    public bool? OnlyActive { get; set; }
}

