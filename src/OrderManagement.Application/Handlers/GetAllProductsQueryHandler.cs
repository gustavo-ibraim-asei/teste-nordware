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
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<GetAllProductsQueryHandler>? _logger;
    private const int CacheExpirationMinutes = 15; // Produtos mudam pouco

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache? cache = null, ILogger<GetAllProductsQueryHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Tentar obter do cache primeiro (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = "products:all";
                string? cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger?.LogDebug("Cache encontrado para lista de produtos");
                    return JsonSerializer.Deserialize<List<ProductDto>>(cachedResult) ?? new List<ProductDto>();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao acessar cache. Continuando sem cache.");
            }
        }

        // Se não estiver em cache, buscar do banco
        IEnumerable<Domain.Entities.Product> products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        List<ProductDto> result = _mapper.Map<List<ProductDto>>(products);

        // Armazenar no cache para próximas requisições (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = "products:all";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    cacheOptions,
                    cancellationToken);
                _logger?.LogDebug("Lista de produtos armazenada no cache");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao armazenar no cache. Continuando sem cache.");
            }
        }

        return result;
    }
}

