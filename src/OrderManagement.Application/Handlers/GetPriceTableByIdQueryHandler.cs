using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetPriceTableByIdQueryHandler : IRequestHandler<GetPriceTableByIdQuery, PriceTableDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPriceTableByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PriceTableDto?> Handle(GetPriceTableByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.PriceTable? priceTable = await _unitOfWork.PriceTables.GetByIdAsync(request.Id, cancellationToken);
        if (priceTable == null)
            return null;

        return _mapper.Map<PriceTableDto>(priceTable);
    }
}

