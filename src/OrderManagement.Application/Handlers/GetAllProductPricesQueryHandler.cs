using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetAllProductPricesQueryHandler : IRequestHandler<GetAllProductPricesQuery, List<ProductPriceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductPricesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ProductPriceDto>> Handle(GetAllProductPricesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<ProductPrice> productPrices;

        if (request.ProductId.HasValue && request.PriceTableId.HasValue)
        {
            // Buscar específico
            ProductPrice? productPrice = await _unitOfWork.ProductPrices.GetByProductAndPriceTableAsync(
                request.ProductId.Value, 
                request.PriceTableId.Value, 
                cancellationToken);
            productPrices = productPrice != null ? new[] { productPrice } : Enumerable.Empty<ProductPrice>();
        }
        else if (request.ProductId.HasValue)
        {
            productPrices = await _unitOfWork.ProductPrices.GetByProductIdAsync(request.ProductId.Value, cancellationToken);
        }
        else if (request.PriceTableId.HasValue)
        {
            productPrices = await _unitOfWork.ProductPrices.GetByPriceTableIdAsync(request.PriceTableId.Value, cancellationToken);
        }
        else
        {
            productPrices = await _unitOfWork.ProductPrices.GetAllAsync(cancellationToken);
        }

        return _mapper.Map<List<ProductPriceDto>>(productPrices);
    }
}

