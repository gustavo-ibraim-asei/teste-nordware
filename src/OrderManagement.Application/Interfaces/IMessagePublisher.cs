using OrderManagement.Domain.Events;

namespace OrderManagement.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}


