using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        string tenantId = _tenantProvider.GetCurrentTenant();

        // Verificar se já existe cliente com o mesmo email no tenant
        Customer? existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Customer.Email, cancellationToken);
        if (existingCustomer != null && existingCustomer.TenantId == tenantId)
        {
            throw new InvalidOperationException($"Já existe um cliente com o email {request.Customer.Email}");
        }

        Customer customer = new Customer(
            request.Customer.Name,
            request.Customer.Email,
            request.Customer.Phone,
            request.Customer.Document,
            tenantId
        );

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }
}

