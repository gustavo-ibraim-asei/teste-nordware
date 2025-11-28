using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetStockOfficeByIdQueryHandler : IRequestHandler<GetStockOfficeByIdQuery, StockOfficeDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStockOfficeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StockOfficeDto?> Handle(GetStockOfficeByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.StockOffice? stockOffice = await _unitOfWork.StockOffices.GetByIdAsync(request.Id, cancellationToken);
        return stockOffice == null ? null : _mapper.Map<StockOfficeDto>(stockOffice);
    }
}

