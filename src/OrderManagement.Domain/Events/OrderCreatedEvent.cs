namespace OrderManagement.Domain.Events;

public class OrderCreatedEvent : IDomainEvent
{
    public int OrderId { get; }
    public int CustomerId { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredOn { get; }

    public OrderCreatedEvent(int orderId, int customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OccurredOn = DateTime.UtcNow;
    }
}


