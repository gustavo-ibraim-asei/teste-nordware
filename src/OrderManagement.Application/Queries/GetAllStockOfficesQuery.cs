using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllStockOfficesQuery : IRequest<List<StockOfficeDto>>
{
}



