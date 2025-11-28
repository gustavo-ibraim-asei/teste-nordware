using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeleteStockOfficeCommandHandler : IRequestHandler<DeleteStockOfficeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStockOfficeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteStockOfficeCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.StockOffice? stockOffice = await _unitOfWork.StockOffices.GetByIdAsync(request.Id, cancellationToken);
        if (stockOffice == null)
            throw new KeyNotFoundException($"Filial com ID {request.Id} não encontrada");

        await _unitOfWork.StockOffices.DeleteAsync(stockOffice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

