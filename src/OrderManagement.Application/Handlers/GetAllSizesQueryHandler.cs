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
public class GetAllSizesQueryHandler : IRequestHandler<GetAllSizesQuery, List<SizeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<GetAllSizesQueryHandler>? _logger;
    private const int CacheExpirationMinutes = 30; // Tamanhos mudam raramente

    public GetAllSizesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache? cache = null, ILogger<GetAllSizesQueryHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<SizeDto>> Handle(GetAllSizesQuery request, CancellationToken cancellationToken)
    {
        // Tentar obter do cache primeiro (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = "sizes:all";
                string? cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger?.LogDebug("Cache encontrado para lista de tamanhos");
                    return JsonSerializer.Deserialize<List<SizeDto>>(cachedResult) ?? new List<SizeDto>();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao acessar cache. Continuando sem cache.");
            }
        }

        // Se não estiver em cache, buscar do banco
        IEnumerable<Domain.Entities.Size> sizes = await _unitOfWork.Sizes.GetAllAsync(cancellationToken);
        List<SizeDto> result = _mapper.Map<List<SizeDto>>(sizes);

        // Armazenar no cache para próximas requisições (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = "sizes:all";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    cacheOptions,
                    cancellationToken);
                _logger?.LogDebug("Lista de tamanhos armazenada no cache");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao armazenar no cache. Continuando sem cache.");
            }
        }

        return result;
    }
}

