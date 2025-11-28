using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Multitenancy;

namespace OrderManagement.Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IOrderFactory _orderFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IShippingCalculationService _shippingService;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDomainEventDispatcher eventDispatcher, IOrderFactory orderFactory, ITenantProvider tenantProvider, IShippingCalculationService shippingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventDispatcher = eventDispatcher;
        _orderFactory = orderFactory;
        _tenantProvider = tenantProvider;
        _shippingService = shippingService;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        string tenantId = _tenantProvider.GetCurrentTenant();
        Domain.Entities.Order order = _orderFactory.CreateOrder(request.Order, tenantId);

        // Calcular e aplicar frete se selecionado (opcional na criação)
        if (request.Order.SelectedCarrierId.HasValue &&
            request.Order.SelectedShippingTypeId.HasValue)
        {
            decimal totalWeight = request.Order.Items.Sum(i => i.Quantity * 1.0m); // Assumindo 1kg por item
            List<Domain.ValueObjects.ShippingOption> shippingOptions = await _shippingService.CalculateShippingOptionsAsync(request.Order.ShippingAddress.ZipCode, order.TotalAmount, totalWeight, cancellationToken);

            Domain.ValueObjects.ShippingOption? selectedOption = shippingOptions.FirstOrDefault(o => o.CarrierId == request.Order.SelectedCarrierId.Value && o.ShippingTypeId == request.Order.SelectedShippingTypeId.Value);

            if (selectedOption != null)
            {
                order.SetShippingInfo(selectedOption.CarrierId, selectedOption.CarrierName, selectedOption.ShippingTypeId, selectedOption.ShippingType, selectedOption.Price, selectedOption.EstimatedDays);
            }
        }

        // Se não selecionado, o pedido é criado sem frete (será obrigatório na finalização)
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain events
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return _mapper.Map<OrderDto>(order);
    }
}

