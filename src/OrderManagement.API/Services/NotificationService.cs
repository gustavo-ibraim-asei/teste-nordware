using Microsoft.AspNetCore.SignalR;
using OrderManagement.API.Hubs;
using OrderManagement.Application.DTOs;

namespace OrderManagement.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly IFeatureFlags _featureFlags;

    public NotificationService(IHubContext<OrderHub> hubContext, IFeatureFlags featureFlags)
    {
        _hubContext = hubContext;
        _featureFlags = featureFlags;
    }

    public async Task NotifyOrderCreatedAsync(OrderDto order, string tenantId, CancellationToken cancellationToken = default)
    {
        if (_featureFlags.IsEnabled("real-time-updates"))
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}").SendAsync("OrderCreated", order, cancellationToken);
        }
    }

    public async Task NotifyOrderStatusUpdatedAsync(OrderDto order, string tenantId, CancellationToken cancellationToken = default)
    {
        if (_featureFlags.IsEnabled("real-time-updates"))
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}").SendAsync("OrderStatusUpdated", order, cancellationToken);
        }
    }
}
