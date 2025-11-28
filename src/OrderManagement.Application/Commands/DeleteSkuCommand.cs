using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteSkuCommand : IRequest<Unit>
{
    public int Id { get; set; }
}



