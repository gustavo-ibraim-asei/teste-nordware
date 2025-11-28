using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetAllSkusQueryHandler : IRequestHandler<GetAllSkusQuery, List<SkuDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSkusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<SkuDto>> Handle(GetAllSkusQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Sku> skus;
        if (request.ProductId.HasValue)
        {
            skus = await _unitOfWork.Skus.GetByProductIdAsync(request.ProductId.Value, cancellationToken);
        }
        else
        {
            skus = await _unitOfWork.Skus.GetAllAsync(cancellationToken);
        }

        return _mapper.Map<List<SkuDto>>(skus);
    }
}



