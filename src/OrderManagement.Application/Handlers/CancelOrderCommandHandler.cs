using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDomainEventDispatcher eventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Order? order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Pedido com ID {request.OrderId} não encontrado");

        order.Cancel(request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain events
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return _mapper.Map<OrderDto>(order);
    }
}

