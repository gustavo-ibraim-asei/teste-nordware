using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateProductPriceCommandHandler : IRequestHandler<CreateProductPriceCommand, ProductPriceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreateProductPriceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<ProductPriceDto> Handle(CreateProductPriceCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe preço para esta combinação
        ProductPrice? existing = await _unitOfWork.ProductPrices.GetByProductAndPriceTableAsync(
            request.ProductPrice.ProductId, 
            request.ProductPrice.PriceTableId, 
            cancellationToken);
        
        if (existing != null)
            throw new InvalidOperationException("Já existe um preço para este produto nesta tabela de preços");

        // Buscar entidades relacionadas
        Product? product = await _unitOfWork.Products.GetByIdAsync(request.ProductPrice.ProductId, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Produto com ID {request.ProductPrice.ProductId} não encontrado");

        PriceTable? priceTable = await _unitOfWork.PriceTables.GetByIdAsync(request.ProductPrice.PriceTableId, cancellationToken);
        if (priceTable == null)
            throw new KeyNotFoundException($"Tabela de preços com ID {request.ProductPrice.PriceTableId} não encontrada");

        string tenantId = _tenantProvider.GetCurrentTenant();
        ProductPrice productPrice = new ProductPrice(product, priceTable, request.ProductPrice.UnitPrice, tenantId);

        await _unitOfWork.ProductPrices.AddAsync(productPrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar com relações para mapear corretamente
        ProductPrice? created = await _unitOfWork.ProductPrices.GetByIdAsync(productPrice.Id, cancellationToken);
        if (created == null)
            throw new InvalidOperationException("Erro ao criar preço do produto");

        return _mapper.Map<ProductPriceDto>(created);
    }
}

