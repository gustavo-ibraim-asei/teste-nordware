using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

/// <summary>
/// Handler com invalidação de cache para escalabilidade
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<DeleteProductCommandHandler>? _logger;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, IDistributedCache? cache = null, ILogger<DeleteProductCommandHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        Product? product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Produto com ID {request.Id} não encontrado");

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidar cache do produto específico e da lista
        if (_cache != null)
        {
            await _cache.RemoveAsync($"product:{request.Id}", cancellationToken);
            await _cache.RemoveAsync("products:all", cancellationToken);
            _logger?.LogDebug("Cache de produto {ProductId} invalidado após exclusão", request.Id);
        }

        return Unit.Value;
    }
}

