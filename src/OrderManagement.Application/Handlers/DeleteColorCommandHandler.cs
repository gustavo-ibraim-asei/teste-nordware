using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeleteColorCommandHandler : IRequestHandler<DeleteColorCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteColorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteColorCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Color? color = await _unitOfWork.Colors.GetByIdAsync(request.Id, cancellationToken);
        if (color == null)
            throw new KeyNotFoundException($"Cor com ID {request.Id} não encontrada");

        await _unitOfWork.Colors.DeleteAsync(color, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

