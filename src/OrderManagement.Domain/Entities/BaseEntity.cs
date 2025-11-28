using OrderManagement.Domain.Events;

namespace OrderManagement.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public string TenantId { get; protected set; } = string.Empty;
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

