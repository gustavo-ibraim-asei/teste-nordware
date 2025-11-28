using MediatR;

namespace OrderManagement.Application.Commands;

public class DeletePriceTableCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

