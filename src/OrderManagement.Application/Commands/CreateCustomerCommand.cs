using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateCustomerCommand : IRequest<CustomerDto>
{
    public CreateCustomerDto Customer { get; set; } = null!;
}

