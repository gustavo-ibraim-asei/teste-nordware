using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetColorByIdQuery : IRequest<ColorDto?>
{
    public int Id { get; set; }
}



