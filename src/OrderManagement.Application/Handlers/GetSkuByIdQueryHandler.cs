using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetSkuByIdQueryHandler : IRequestHandler<GetSkuByIdQuery, SkuDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSkuByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SkuDto?> Handle(GetSkuByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Sku? sku = await _unitOfWork.Skus.GetByIdAsync(request.Id, cancellationToken);
        return sku == null ? null : _mapper.Map<SkuDto>(sku);
    }
}

