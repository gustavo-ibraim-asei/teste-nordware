using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateStockOfficeCommandHandler : IRequestHandler<CreateStockOfficeCommand, StockOfficeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreateStockOfficeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<StockOfficeDto> Handle(CreateStockOfficeCommand request, CancellationToken cancellationToken)
    {
        string tenantId = _tenantProvider.GetCurrentTenant();
        StockOffice stockOffice = new StockOffice(
            request.StockOffice.Name,
            request.StockOffice.Code,
            tenantId);

        await _unitOfWork.StockOffices.AddAsync(stockOffice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockOfficeDto>(stockOffice);
    }
}

