using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class UpdateSkuCommandHandler : IRequestHandler<UpdateSkuCommand, SkuDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSkuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SkuDto> Handle(UpdateSkuCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Sku? sku = await _unitOfWork.Skus.GetByIdAsync(request.Id, cancellationToken);
        if (sku == null)
            throw new KeyNotFoundException($"SKU com ID {request.Id} não encontrado");

        sku.UpdateBarcode(request.Sku.Barcode);

        await _unitOfWork.Skus.UpdateAsync(sku, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SkuDto>(sku);
    }
}

