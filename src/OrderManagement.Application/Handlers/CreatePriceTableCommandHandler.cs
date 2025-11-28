using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreatePriceTableCommandHandler : IRequestHandler<CreatePriceTableCommand, PriceTableDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreatePriceTableCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<PriceTableDto> Handle(CreatePriceTableCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe uma tabela de preços com o mesmo nome
        PriceTable? existing = await _unitOfWork.PriceTables.GetByNameAsync(request.PriceTable.Name, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Já existe uma tabela de preços com o nome '{request.PriceTable.Name}'");

        string tenantId = _tenantProvider.GetCurrentTenant();
        PriceTable priceTable = new PriceTable(request.PriceTable.Name, request.PriceTable.Description, tenantId);

        await _unitOfWork.PriceTables.AddAsync(priceTable, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PriceTableDto>(priceTable);
    }
}

