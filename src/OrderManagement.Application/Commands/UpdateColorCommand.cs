using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateColorCommand : IRequest<ColorDto>
{
    public int Id { get; set; }
    public UpdateColorDto Color { get; set; } = null!;
}



