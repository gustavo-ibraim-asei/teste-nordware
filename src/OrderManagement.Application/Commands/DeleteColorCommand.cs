using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteColorCommand : IRequest<Unit>
{
    public int Id { get; set; }
}



