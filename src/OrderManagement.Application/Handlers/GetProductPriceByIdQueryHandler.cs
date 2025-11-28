using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetProductPriceByIdQueryHandler : IRequestHandler<GetProductPriceByIdQuery, ProductPriceDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductPriceByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductPriceDto?> Handle(GetProductPriceByIdQuery request, CancellationToken cancellationToken)
    {
        ProductPrice? productPrice = await _unitOfWork.ProductPrices.GetByIdAsync(request.Id, cancellationToken);
        if (productPrice == null)
            return null;

        return _mapper.Map<ProductPriceDto>(productPrice);
    }
}

