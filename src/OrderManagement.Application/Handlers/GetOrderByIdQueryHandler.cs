using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Order? order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }
}

