using MediatR;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Commands;

public class ProcessOrdersBatchCommand : IRequest<BatchProcessResultDto>
{
    public List<int> OrderIds { get; set; } = new();
    public OrderStatus TargetStatus { get; set; }
}

public class BatchProcessResultDto
{
    public int TotalProcessed { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<BatchProcessItemDto> Results { get; set; } = new();
}

public class BatchProcessItemDto
{
    public int OrderId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}





