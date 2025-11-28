using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        List<Customer> customers;

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            customers = await _unitOfWork.Customers.GetByNameAsync(request.Name, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.Email))
        {
            Customer? customer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
            customers = customer != null ? new List<Customer> { customer } : new List<Customer>();
        }
        else
        {
            customers = (await _unitOfWork.Customers.GetAllAsync(cancellationToken)).ToList();
        }

        return _mapper.Map<List<CustomerDto>>(customers);
    }
}

