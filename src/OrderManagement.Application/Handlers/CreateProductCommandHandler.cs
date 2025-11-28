using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

/// <summary>
/// Handler com invalidação de cache para escalabilidade
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<CreateProductCommandHandler>? _logger;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider, IDistributedCache? cache = null, ILogger<CreateProductCommandHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        string tenantId = _tenantProvider.GetCurrentTenant();
        Product product = new Product(request.Product.Name, request.Product.Code, request.Product.Description, tenantId);

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        ProductDto dto = _mapper.Map<ProductDto>(product);

        // Invalidar cache de lista de produtos (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                await _cache.RemoveAsync("products:all", cancellationToken);
                _logger?.LogDebug("Cache de lista de produtos invalidado após criação de novo produto");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao invalidar cache. Continuando sem cache.");
            }
        }

        return dto;
    }
}

