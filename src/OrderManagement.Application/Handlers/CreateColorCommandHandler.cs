using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateColorCommandHandler : IRequestHandler<CreateColorCommand, ColorDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantProvider _tenantProvider;

    public CreateColorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantProvider = tenantProvider;
    }

    public async Task<ColorDto> Handle(CreateColorCommand request, CancellationToken cancellationToken)
    {
        string tenantId = _tenantProvider.GetCurrentTenant();
        Color color = new Color(request.Color.Name, request.Color.Code, tenantId);

        await _unitOfWork.Colors.AddAsync(color, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ColorDto>(color);
    }
}

