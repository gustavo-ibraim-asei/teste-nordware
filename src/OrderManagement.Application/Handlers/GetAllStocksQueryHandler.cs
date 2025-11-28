using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetAllStocksQueryHandler : IRequestHandler<GetAllStocksQuery, List<StockDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStocksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<StockDto>> Handle(GetAllStocksQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Stock> stocks;
        if (request.SkuId.HasValue)
        {
            stocks = await _unitOfWork.Stocks.GetBySkuAsync(request.SkuId.Value, cancellationToken);
        }
        else if (request.StockOfficeId.HasValue)
        {
            stocks = await _unitOfWork.Stocks.GetByStockOfficeAsync(request.StockOfficeId.Value, cancellationToken);
        }
        else
        {
            stocks = await _unitOfWork.Stocks.GetAllAsync(cancellationToken);
        }

        List<StockDto> stockDtos = _mapper.Map<List<StockDto>>(stocks);
        foreach (StockDto stockDto in stockDtos)
        {
            Domain.Entities.Stock stock = stocks.First(s => s.Id == stockDto.Id);
            stockDto.AvailableQuantity = stock.AvailableQuantity;
        }

        return stockDtos;
    }
}

