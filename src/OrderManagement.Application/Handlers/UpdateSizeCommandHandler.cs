using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdateSizeCommandHandler : IRequestHandler<UpdateSizeCommand, SizeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSizeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SizeDto> Handle(UpdateSizeCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Size? size = await _unitOfWork.Sizes.GetByIdAsync(request.Id, cancellationToken);
        if (size == null)
            throw new KeyNotFoundException($"Tamanho com ID {request.Id} não encontrado");

        size.UpdateName(request.Size.Name);
        size.UpdateCode(request.Size.Code);

        await _unitOfWork.Sizes.UpdateAsync(size, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SizeDto>(size);
    }
}

