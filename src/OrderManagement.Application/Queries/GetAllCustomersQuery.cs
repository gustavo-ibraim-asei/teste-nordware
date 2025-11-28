using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries;

public class GetAllCustomersQuery : IRequest<List<CustomerDto>>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}

