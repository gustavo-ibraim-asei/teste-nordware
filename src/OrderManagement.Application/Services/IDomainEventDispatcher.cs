using OrderManagement.Domain.Events;

namespace OrderManagement.Application.Services;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}


