using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateSizeCommand : IRequest<SizeDto>
{
    public CreateSizeDto Size { get; set; } = null!;
}



