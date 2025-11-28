using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, StockDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreateStockCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<StockDto> Handle(CreateStockCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe estoque para este SKU e Filial
        Stock? existingStock = await _unitOfWork.Stocks.GetBySkuAndOfficeAsync(
            request.Stock.SkuId,
            request.Stock.StockOfficeId,
            cancellationToken);

        if (existingStock != null)
            throw new InvalidOperationException("Estoque já existe para este SKU e Filial");

        string tenantId = _tenantProvider.GetCurrentTenant();
        Stock stock = new Stock(
            request.Stock.SkuId,
            request.Stock.StockOfficeId,
            request.Stock.Quantity,
            tenantId);

        await _unitOfWork.Stocks.AddAsync(stock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Buscar novamente para incluir as propriedades de navegação (StockOffice, Sku)
        stock = await _unitOfWork.Stocks.GetByIdAsync(stock.Id, cancellationToken);
        StockDto stockDto = _mapper.Map<StockDto>(stock!);
        stockDto.AvailableQuantity = stock.AvailableQuantity;
        return stockDto;
    }
}

