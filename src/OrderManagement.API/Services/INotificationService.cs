using OrderManagement.Application.DTOs;

namespace OrderManagement.API.Services;

public interface INotificationService
{
    Task NotifyOrderCreatedAsync(OrderDto order, string tenantId, CancellationToken cancellationToken = default);
    Task NotifyOrderStatusUpdatedAsync(OrderDto order, string tenantId, CancellationToken cancellationToken = default);
}


