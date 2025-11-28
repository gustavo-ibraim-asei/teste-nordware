using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllSkusQuery : IRequest<List<SkuDto>>
{
    public int? ProductId { get; set; }
}



