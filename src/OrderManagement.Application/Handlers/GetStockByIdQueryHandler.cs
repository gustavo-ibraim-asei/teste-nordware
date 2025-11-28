using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStockByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StockDto?> Handle(GetStockByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Stock? stock = await _unitOfWork.Stocks.GetByIdAsync(request.Id, cancellationToken);
        if (stock == null)
            return null;

        StockDto stockDto = _mapper.Map<StockDto>(stock);
        stockDto.AvailableQuantity = stock.AvailableQuantity;
        return stockDto;
    }
}

