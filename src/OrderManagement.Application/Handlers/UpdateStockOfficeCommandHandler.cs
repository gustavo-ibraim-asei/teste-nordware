using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdateStockOfficeCommandHandler : IRequestHandler<UpdateStockOfficeCommand, StockOfficeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateStockOfficeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<StockOfficeDto> Handle(UpdateStockOfficeCommand request, CancellationToken cancellationToken)
    {
        StockOffice? stockOffice = await _unitOfWork.StockOffices.GetByIdAsync(request.Id, cancellationToken);
        if (stockOffice == null)
            throw new KeyNotFoundException($"Filial com ID {request.Id} não encontrada");

        stockOffice.UpdateName(request.StockOffice.Name);
        stockOffice.UpdateCode(request.StockOffice.Code);

        await _unitOfWork.StockOffices.UpdateAsync(stockOffice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StockOfficeDto>(stockOffice);
    }
}

