using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteStockCommand : IRequest<Unit>
{
    public int Id { get; set; }
}



