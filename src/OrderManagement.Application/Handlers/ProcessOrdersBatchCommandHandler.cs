using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Application.Handlers;

public class ProcessOrdersBatchCommandHandler : IRequestHandler<ProcessOrdersBatchCommand, BatchProcessResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessOrdersBatchCommandHandler> _logger;

    public ProcessOrdersBatchCommandHandler(IUnitOfWork unitOfWork, ILogger<ProcessOrdersBatchCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BatchProcessResultDto> Handle(ProcessOrdersBatchCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando lote de {Count} pedidos", request.OrderIds.Count);

        List<BatchProcessItemDto> results = new List<BatchProcessItemDto>();
        IEnumerable<Task<BatchProcessItemDto>> tasks = request.OrderIds.Select(async orderId =>
        {
            try
            {
                Domain.Entities.Order? order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
                if (order == null)
                {
                    return new BatchProcessItemDto
                    {
                        OrderId = orderId,
                        Success = false,
                        ErrorMessage = "Pedido não encontrado"
                    };
                }

                order.UpdateStatus(request.TargetStatus);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new BatchProcessItemDto
                {
                    OrderId = orderId,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido {OrderId} no lote", orderId);
                return new BatchProcessItemDto
                {
                    OrderId = orderId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        });

        // Process all orders in parallel
        BatchProcessItemDto[] taskResults = await Task.WhenAll(tasks);
        results.AddRange(taskResults);

        int successful = results.Count(r => r.Success);
        int failed = results.Count(r => !r.Success);

        _logger.LogInformation("Processamento em lote concluído. Sucesso: {Successful}, Falhas: {Failed}", successful, failed);

        return new BatchProcessResultDto
        {
            TotalProcessed = results.Count,
            Successful = successful,
            Failed = failed,
            Results = results
        };
    }
}

