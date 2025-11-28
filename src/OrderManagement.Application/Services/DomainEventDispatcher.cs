using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Events;

namespace OrderManagement.Application.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMessagePublisher _messagePublisher;

    public DomainEventDispatcher(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, cancellationToken);
        }
    }
}

