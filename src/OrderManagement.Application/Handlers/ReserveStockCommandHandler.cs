using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, StockDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStockService _stockService;

    public ReserveStockCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IStockService stockService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stockService = stockService;
    }

    public async Task<StockDto> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Stock? stock = await _unitOfWork.Stocks.GetByIdAsync(request.StockId, cancellationToken);
        if (stock == null)
            throw new KeyNotFoundException($"Estoque com ID {request.StockId} não encontrado");

        await _stockService.ReserveStockAsync(stock.SkuId, stock.StockOfficeId, request.Reserve.Quantity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stock = await _unitOfWork.Stocks.GetByIdAsync(request.StockId, cancellationToken);
        StockDto stockDto = _mapper.Map<StockDto>(stock!);
        stockDto.AvailableQuantity = stock.AvailableQuantity;
        return stockDto;
    }
}

