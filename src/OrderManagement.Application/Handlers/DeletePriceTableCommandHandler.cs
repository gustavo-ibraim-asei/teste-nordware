using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeletePriceTableCommandHandler : IRequestHandler<DeletePriceTableCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePriceTableCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeletePriceTableCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.PriceTable? priceTable = await _unitOfWork.PriceTables.GetByIdAsync(request.Id, cancellationToken);
        if (priceTable == null)
            throw new KeyNotFoundException($"Tabela de preços com ID {request.Id} não encontrada");

        await _unitOfWork.PriceTables.DeleteAsync(priceTable, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

