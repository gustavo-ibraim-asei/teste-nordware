using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateCustomerCommand : IRequest<CustomerDto>
{
    public int Id { get; set; }
    public UpdateCustomerDto Customer { get; set; } = null!;
}

