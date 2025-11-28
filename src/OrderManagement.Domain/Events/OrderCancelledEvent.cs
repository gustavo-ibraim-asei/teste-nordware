namespace OrderManagement.Domain.Events;

public class OrderCancelledEvent : IDomainEvent
{
    public int OrderId { get; }
    public int CustomerId { get; }
    public string? Reason { get; }
    public DateTime OccurredOn { get; }

    public OrderCancelledEvent(int orderId, int customerId, string? reason = null)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}


