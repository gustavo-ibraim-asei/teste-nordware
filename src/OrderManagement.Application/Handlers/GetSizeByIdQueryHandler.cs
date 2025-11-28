using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetSizeByIdQueryHandler : IRequestHandler<GetSizeByIdQuery, SizeDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSizeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SizeDto?> Handle(GetSizeByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Size? size = await _unitOfWork.Sizes.GetByIdAsync(request.Id, cancellationToken);
        return size == null ? null : _mapper.Map<SizeDto>(size);
    }
}

