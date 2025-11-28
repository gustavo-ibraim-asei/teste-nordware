using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdatePriceTableCommandHandler : IRequestHandler<UpdatePriceTableCommand, PriceTableDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePriceTableCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PriceTableDto> Handle(UpdatePriceTableCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.PriceTable? priceTable = await _unitOfWork.PriceTables.GetByIdAsync(request.Id, cancellationToken);
        if (priceTable == null)
            throw new KeyNotFoundException($"Tabela de preços com ID {request.Id} não encontrada");

        priceTable.UpdateName(request.PriceTable.Name);
        if (request.PriceTable.Description != null)
            priceTable.UpdateDescription(request.PriceTable.Description);

        if (request.PriceTable.IsActive.HasValue)
        {
            if (request.PriceTable.IsActive.Value)
                priceTable.Activate();
            else
                priceTable.Deactivate();
        }

        await _unitOfWork.PriceTables.UpdateAsync(priceTable, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PriceTableDto>(priceTable);
    }
}

