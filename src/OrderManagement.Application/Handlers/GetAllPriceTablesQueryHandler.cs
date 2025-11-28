using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetAllPriceTablesQueryHandler : IRequestHandler<GetAllPriceTablesQuery, List<PriceTableDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPriceTablesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PriceTableDto>> Handle(GetAllPriceTablesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.PriceTable> priceTables;

        if (request.OnlyActive == true)
        {
            priceTables = await _unitOfWork.PriceTables.GetActivePriceTablesAsync(cancellationToken);
        }
        else
        {
            priceTables = await _unitOfWork.PriceTables.GetAllAsync(cancellationToken);
        }

        return _mapper.Map<List<PriceTableDto>>(priceTables);
    }
}

