using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateColorCommand : IRequest<ColorDto>
{
    public CreateColorDto Color { get; set; } = null!;
}



