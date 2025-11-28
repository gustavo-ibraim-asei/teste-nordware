using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteSizeCommand : IRequest<Unit>
{
    public int Id { get; set; }
}



