using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, ProductPriceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductPriceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductPriceDto> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        ProductPrice? productPrice = await _unitOfWork.ProductPrices.GetByIdAsync(request.Id, cancellationToken);
        if (productPrice == null)
            throw new KeyNotFoundException($"Preço do produto com ID {request.Id} não encontrado");

        productPrice.UpdatePrice(request.ProductPrice.UnitPrice);

        await _unitOfWork.ProductPrices.UpdateAsync(productPrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarregar com relações
        ProductPrice? updated = await _unitOfWork.ProductPrices.GetByIdAsync(request.Id, cancellationToken);
        if (updated == null)
            throw new InvalidOperationException("Erro ao atualizar preço do produto");

        return _mapper.Map<ProductPriceDto>(updated);
    }
}