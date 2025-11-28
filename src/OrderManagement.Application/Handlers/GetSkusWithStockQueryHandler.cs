using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetSkusWithStockQueryHandler : IRequestHandler<GetSkusWithStockQuery, List<SkuWithStockDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSkusWithStockQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<SkuWithStockDto>> Handle(GetSkusWithStockQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Sku> allSkus = await _unitOfWork.Skus.GetAllAsync(cancellationToken);
        List<SkuWithStockDto> skusWithStock = new List<SkuWithStockDto>();

        foreach (Domain.Entities.Sku sku in allSkus)
        {
            IEnumerable<Domain.Entities.Stock> stocks = await _unitOfWork.Stocks.GetBySkuAsync(sku.Id, cancellationToken);
            int totalAvailable = stocks.Sum(s => s.AvailableQuantity);

            if (totalAvailable > 0)
            {
                SkuDto skuDto = _mapper.Map<SkuDto>(sku);
                skusWithStock.Add(new SkuWithStockDto
                {
                    Sku = skuDto,
                    TotalAvailableQuantity = totalAvailable,
                    Stocks = stocks.Select(s => new StockInfoDto
                    {
                        StockOfficeId = s.StockOfficeId,
                        StockOfficeName = s.StockOffice.Name,
                        AvailableQuantity = s.AvailableQuantity
                    }).ToList()
                });
            }
        }

        return skusWithStock;
    }
}

