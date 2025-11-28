using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteStockOfficeCommand : IRequest<Unit>
{
    public int Id { get; set; }
}



