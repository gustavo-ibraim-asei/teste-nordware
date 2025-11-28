using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStockCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteStockCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Stock? stock = await _unitOfWork.Stocks.GetByIdAsync(request.Id, cancellationToken);
        if (stock == null)
            throw new KeyNotFoundException($"Estoque com ID {request.Id} não encontrado");

        await _unitOfWork.Stocks.DeleteAsync(stock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

