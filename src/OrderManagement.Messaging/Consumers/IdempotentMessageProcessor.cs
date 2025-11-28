using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Messaging.Consumers;

public class IdempotentMessageProcessor
{
    private readonly OrderManagementDbContext _dbContext;
    private readonly ILogger<IdempotentMessageProcessor> _logger;

    public IdempotentMessageProcessor(OrderManagementDbContext dbContext, ILogger<IdempotentMessageProcessor> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> IsMessageProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProcessedMessage>()
            .AnyAsync(m => m.MessageId == messageId, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(string messageId, string eventType, string tenantId, CancellationToken cancellationToken = default)
    {
        ProcessedMessage processedMessage = new ProcessedMessage
        {
            MessageId = messageId,
            EventType = eventType,
            ProcessedAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        _dbContext.Set<ProcessedMessage>().Add(processedMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Mensagem {MessageId} marcada como processada", messageId);
    }
}

