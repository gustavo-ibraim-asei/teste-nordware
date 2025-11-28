using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

/// <summary>
/// Handler com invalidação de cache para escalabilidade
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<UpdateProductCommandHandler>? _logger;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache? cache = null, ILogger<UpdateProductCommandHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        Product? product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Produto com ID {request.Id} não encontrado");

        product.UpdateName(request.Product.Name);
        product.UpdateCode(request.Product.Code);
        product.UpdateDescription(request.Product.Description);

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        ProductDto dto = _mapper.Map<ProductDto>(product);

        // Invalidar cache do produto específico e da lista
        if (_cache != null)
        {
            await _cache.RemoveAsync($"product:{request.Id}", cancellationToken);
            await _cache.RemoveAsync("products:all", cancellationToken);
            _logger?.LogDebug("Cache de produto {ProductId} invalidado após atualização", request.Id);
        }

        return dto;
    }
}

