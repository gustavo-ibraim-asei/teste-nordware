using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetColorByIdQueryHandler : IRequestHandler<GetColorByIdQuery, ColorDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetColorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ColorDto?> Handle(GetColorByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Color? color = await _unitOfWork.Colors.GetByIdAsync(request.Id, cancellationToken);
        return color == null ? null : _mapper.Map<ColorDto>(color);
    }
}

