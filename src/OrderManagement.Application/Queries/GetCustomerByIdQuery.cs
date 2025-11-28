using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetCustomerByIdQuery : IRequest<CustomerDto?>
{
    public int Id { get; set; }
}

