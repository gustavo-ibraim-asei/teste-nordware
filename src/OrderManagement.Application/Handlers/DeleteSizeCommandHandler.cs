using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class DeleteSizeCommandHandler : IRequestHandler<DeleteSizeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSizeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteSizeCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Size? size = await _unitOfWork.Sizes.GetByIdAsync(request.Id, cancellationToken);
        if (size == null)
            throw new KeyNotFoundException($"Tamanho com ID {request.Id} não encontrado");

        await _unitOfWork.Sizes.DeleteAsync(size, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

