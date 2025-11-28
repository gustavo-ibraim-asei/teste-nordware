namespace OrderManagement.Infrastructure.Data;

public class ProcessedMessage
{
    public int Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string TenantId { get; set; } = string.Empty;
}





