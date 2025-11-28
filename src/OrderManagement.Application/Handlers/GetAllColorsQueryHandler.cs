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
public class GetAllColorsQueryHandler : IRequestHandler<GetAllColorsQuery, List<ColorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<GetAllColorsQueryHandler>? _logger;
    private const int CacheExpirationMinutes = 30; // Cores mudam raramente

    public GetAllColorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache? cache = null, ILogger<GetAllColorsQueryHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ColorDto>> Handle(GetAllColorsQuery request, CancellationToken cancellationToken)
    {
        // Tentar obter do cache primeiro (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = "colors:all";
                string? cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger?.LogDebug("Cache encontrado para lista de cores");
                    return JsonSerializer.Deserialize<List<ColorDto>>(cachedResult) ?? new List<ColorDto>();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao acessar cache. Continuando sem cache.");
            }
        }

        // Se não estiver em cache, buscar do banco
        IEnumerable<Domain.Entities.Color> colors = await _unitOfWork.Colors.GetAllAsync(cancellationToken);
        List<ColorDto> result = _mapper.Map<List<ColorDto>>(colors);

        // Armazenar no cache para próximas requisições (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = "colors:all";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    cacheOptions,
                    cancellationToken);
                _logger?.LogDebug("Lista de cores armazenada no cache");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao armazenar no cache. Continuando sem cache.");
            }
        }

        return result;
    }
}

