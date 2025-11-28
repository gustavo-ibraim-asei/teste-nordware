using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetSkuByIdQuery : IRequest<SkuDto?>
{
    public int Id { get; set; }
}



