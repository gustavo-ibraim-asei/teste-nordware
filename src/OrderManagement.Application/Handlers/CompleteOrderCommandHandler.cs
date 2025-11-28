using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IShippingCalculationService _shippingService;

    public CompleteOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDomainEventDispatcher eventDispatcher, IShippingCalculationService shippingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventDispatcher = eventDispatcher;
        _shippingService = shippingService;
    }

    public async Task<OrderDto> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Order? order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Pedido com ID {request.OrderId} não encontrado");

        if (order.Status != Domain.Enums.OrderStatus.Pending)
            throw new InvalidOperationException("Apenas pedidos pendentes podem ser finalizados");

        // Calcular opções de frete
        decimal totalWeight = order.Items.Sum(i => i.Quantity * 1.0m); // Assumindo 1kg por item
        List<Domain.ValueObjects.ShippingOption> shippingOptions = await _shippingService.CalculateShippingOptionsAsync(order.ShippingAddress.ZipCode, order.TotalAmount, totalWeight, cancellationToken);

        // Buscar opção selecionada
        Domain.ValueObjects.ShippingOption? selectedOption = shippingOptions.FirstOrDefault(o => 
            o.CarrierId == request.ShippingInfo.CarrierId &&
            o.ShippingTypeId == request.ShippingInfo.ShippingTypeId);

        if (selectedOption == null)
            throw new ArgumentException("Opção de frete selecionada não encontrada ou não disponível");

        // Aplicar frete ao pedido
        order.SetShippingInfo(selectedOption.CarrierId, selectedOption.CarrierName, selectedOption.ShippingTypeId, selectedOption.ShippingType, selectedOption.Price, selectedOption.EstimatedDays);

        // Atualizar status para Confirmed
        order.UpdateStatus(Domain.Enums.OrderStatus.Confirmed);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain events
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return _mapper.Map<OrderDto>(order);
    }
}

