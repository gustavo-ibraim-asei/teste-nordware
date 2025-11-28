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
        try
        {
            if (_featureFlags.IsEnabled("real-time-updates"))
            {
                string groupName = $"tenant_{tenantId}";
                await _hubContext.Clients.Group(groupName).SendAsync("OrderCreated", order, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification failure shouldn't break order creation
            Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
        }
    }

    public async Task NotifyOrderStatusUpdatedAsync(OrderDto order, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_featureFlags.IsEnabled("real-time-updates"))
            {
                string groupName = $"tenant_{tenantId}";
                await _hubContext.Clients.Group(groupName).SendAsync("OrderStatusUpdated", order, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - notification failure shouldn't break order update
            Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
        }
    }
}



