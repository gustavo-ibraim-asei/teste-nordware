using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeleteProductPriceCommandHandler : IRequestHandler<DeleteProductPriceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductPriceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteProductPriceCommand request, CancellationToken cancellationToken)
    {
        ProductPrice? productPrice = await _unitOfWork.ProductPrices.GetByIdAsync(request.Id, cancellationToken);
        if (productPrice == null)
            throw new KeyNotFoundException($"Preço do produto com ID {request.Id} não encontrado");

        await _unitOfWork.ProductPrices.DeleteAsync(productPrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

