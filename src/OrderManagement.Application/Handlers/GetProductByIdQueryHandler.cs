using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;
using System.Text.Json;

namespace OrderManagement.Application.Handlers;

/// <summary>
/// Handler otimizado com cache distribuído para escalabilidade
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<GetProductByIdQueryHandler>? _logger;
    private const int CacheExpirationMinutes = 15; // Produtos mudam pouco

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache? cache = null, ILogger<GetProductByIdQueryHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // Tentar obter do cache primeiro (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = $"product:{request.Id}";
                string? cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger?.LogDebug("Cache encontrado para produto {ProductId}", request.Id);
                    return JsonSerializer.Deserialize<ProductDto>(cachedResult);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao acessar cache. Continuando sem cache.");
            }
        }

        // Se não estiver em cache, buscar do banco
        Domain.Entities.Product? product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null) return null;

        ProductDto? dto = _mapper.Map<ProductDto>(product);

        // Armazenar no cache para próximas requisições (com tratamento de erro)
        if (_cache != null && dto != null)
        {
            try
            {
                string cacheKey = $"product:{request.Id}";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(dto),
                    cacheOptions,
                    cancellationToken);
                _logger?.LogDebug("Produto {ProductId} armazenado no cache", request.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao armazenar no cache. Continuando sem cache.");
            }
        }

        return dto;
    }
}

