using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, StockDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateStockCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StockDto> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Stock? stock = await _unitOfWork.Stocks.GetByIdAsync(request.Id, cancellationToken);
        if (stock == null)
            throw new KeyNotFoundException($"Estoque com ID {request.Id} não encontrado");

        stock.UpdateQuantity(request.Stock.Quantity);

        await _unitOfWork.Stocks.UpdateAsync(stock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Buscar novamente para incluir as propriedades de navegação (StockOffice, Sku)
        stock = await _unitOfWork.Stocks.GetByIdAsync(request.Id, cancellationToken);
        StockDto stockDto = _mapper.Map<StockDto>(stock!);
        stockDto.AvailableQuantity = stock.AvailableQuantity;
        return stockDto;
    }
}

