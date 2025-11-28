using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateSizeCommand : IRequest<SizeDto>
{
    public int Id { get; set; }
    public UpdateSizeDto Size { get; set; } = null!;
}



