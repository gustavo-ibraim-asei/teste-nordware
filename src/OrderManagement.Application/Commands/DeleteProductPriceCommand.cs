using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteProductPriceCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

