using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetAllStockOfficesQueryHandler : IRequestHandler<GetAllStockOfficesQuery, List<StockOfficeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStockOfficesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<StockOfficeDto>> Handle(GetAllStockOfficesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.StockOffice> stockOffices = await _unitOfWork.StockOffices.GetAllAsync(cancellationToken);
        return _mapper.Map<List<StockOfficeDto>>(stockOffices);
    }
}

