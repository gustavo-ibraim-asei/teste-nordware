using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetSizeByIdQuery : IRequest<SizeDto?>
{
    public int Id { get; set; }
}



