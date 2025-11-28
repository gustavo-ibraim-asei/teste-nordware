using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeleteSkuCommandHandler : IRequestHandler<DeleteSkuCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSkuCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteSkuCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Sku? sku = await _unitOfWork.Skus.GetByIdAsync(request.Id, cancellationToken);
        if (sku == null)
            throw new KeyNotFoundException($"SKU com ID {request.Id} não encontrado");

        await _unitOfWork.Skus.DeleteAsync(sku, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

