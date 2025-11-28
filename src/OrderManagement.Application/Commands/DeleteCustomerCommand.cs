using MediatR;

namespace OrderManagement.Application.Commands;

public class DeleteCustomerCommand : IRequest
{
    public int Id { get; set; }
}

