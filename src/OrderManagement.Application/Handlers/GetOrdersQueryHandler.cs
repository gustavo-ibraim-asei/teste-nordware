using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;
using System.Text.Json;

namespace OrderManagement.Application.Handlers;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResultDto<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IDistributedCache? cache = null, ILogger<GetOrdersQueryHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResultDto<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Try to get from cache if available (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = $"orders:{request.Query.CustomerId}:{request.Query.Status}:{request.Query.Page}:{request.Query.PageSize}";
                string? cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    _logger?.LogDebug("Cache encontrado para consulta de pedidos");
                    return JsonSerializer.Deserialize<PagedResultDto<OrderDto>>(cachedResult)!;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao acessar cache. Continuando sem cache.");
            }
        }

        // Get all orders first, then filter in memory (simplified approach)
        // In a real scenario, we'd use IQueryable from the repository
        IEnumerable<Domain.Entities.Order> allOrders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        IQueryable<Domain.Entities.Order> query = allOrders.AsQueryable();

        // Apply filters
        if (request.Query.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.Query.CustomerId.Value);
        }

        if (request.Query.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Query.Status.Value);
        }

        if (request.Query.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.Query.StartDate.Value);
        }

        if (request.Query.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.Query.EndDate.Value);
        }

        // Get total count before pagination
        int totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.Query.SortBy?.ToLower() switch
        {
            "createdat" => request.Query.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt),
            "totalamount" => request.Query.SortDescending
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "status" => request.Query.SortDescending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        // Apply pagination
        List<Domain.Entities.Order> items = await query
            .Skip((request.Query.Page - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize)
            .ToListAsync(cancellationToken);

        PagedResultDto<OrderDto> result = new PagedResultDto<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            TotalCount = totalCount,
            Page = request.Query.Page,
            PageSize = request.Query.PageSize
        };

        // Cache result if cache is available (com tratamento de erro)
        if (_cache != null)
        {
            try
            {
                string cacheKey = $"orders:{request.Query.CustomerId}:{request.Query.Status}:{request.Query.Page}:{request.Query.PageSize}";
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    cacheOptions,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao armazenar no cache. Continuando sem cache.");
            }
        }

        return result;
    }
}

