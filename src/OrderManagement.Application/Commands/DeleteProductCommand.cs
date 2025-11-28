using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteProductCommand : IRequest<Unit>
{
    public int Id { get; set; }
}



