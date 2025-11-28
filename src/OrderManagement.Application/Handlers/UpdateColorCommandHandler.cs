using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdateColorCommandHandler : IRequestHandler<UpdateColorCommand, ColorDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateColorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ColorDto> Handle(UpdateColorCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Color? color = await _unitOfWork.Colors.GetByIdAsync(request.Id, cancellationToken);
        if (color == null)
            throw new KeyNotFoundException($"Cor com ID {request.Id} não encontrada");

        color.UpdateName(request.Color.Name);
        color.UpdateCode(request.Color.Code);

        await _unitOfWork.Colors.UpdateAsync(color, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ColorDto>(color);
    }
}

