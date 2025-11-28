using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // Try to get tenant from header
        string? tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        // If not in header, try to get from JWT claim (will be set by JWT middleware)
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = context.User?.FindFirst("tenant_id")?.Value;
        }

        // Default tenant if not provided
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = "default";
        }

        tenantProvider.SetTenant(tenantId);
        context.Items["TenantId"] = tenantId;

        _logger.LogDebug("Tenant definido para: {TenantId}", tenantId);

        await _next(context);
    }
}

