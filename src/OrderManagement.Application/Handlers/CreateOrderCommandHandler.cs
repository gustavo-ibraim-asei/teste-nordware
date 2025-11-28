using AutoMapper;
using MediatR;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IOrderFactory _orderFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly IShippingCalculationService _shippingService;
    private readonly IStockService _stockService;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDomainEventDispatcher eventDispatcher, IOrderFactory orderFactory, ITenantProvider tenantProvider, IShippingCalculationService shippingService, IStockService stockService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventDispatcher = eventDispatcher;
        _orderFactory = orderFactory;
        _tenantProvider = tenantProvider;
        _shippingService = shippingService;
        _stockService = stockService;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        string tenantId = _tenantProvider.GetCurrentTenant();

        // Validar estoque e atribuir SkuId e StockOfficeId para cada item
        foreach (CreateOrderItemDto itemDto in request.Order.Items)
        {
            // Verificar disponibilidade de estoque
            StockAvailabilityResult? availability = await _stockService.CheckAvailabilityAsync(itemDto.ProductId, itemDto.ColorId, itemDto.SizeId, itemDto.Quantity, cancellationToken);

            if (availability == null)
            {
                throw new InvalidOperationException($"Estoque insuficiente para o produto {itemDto.ProductId}, cor {itemDto.ColorId}, tamanho {itemDto.SizeId}, quantidade {itemDto.Quantity}");
            }

            // Buscar SKU existente (não criar aqui)
            Sku? sku = await _unitOfWork.Skus.GetByProductColorSizeAsync(itemDto.ProductId, itemDto.ColorId, itemDto.SizeId, cancellationToken);

            if (sku == null)
            {
                throw new InvalidOperationException($"SKU não encontrado para o produto {itemDto.ProductId}, cor {itemDto.ColorId}, tamanho {itemDto.SizeId}");
            }

            // Atualizar itemDto com SkuId para uso no factory
            itemDto.SkuId = sku.Id;
            itemDto.StockOfficeId = availability.StockOfficeId;
        }

        Domain.Entities.Order order = _orderFactory.CreateOrder(request.Order, tenantId);

        // Atribuir SkuId e StockOfficeId aos itens do pedido
        for (int i = 0; i < order.Items.Count; i++)
        {
            OrderItem item = order.Items.ElementAt(i);
            CreateOrderItemDto itemDto = request.Order.Items[i];
            item.SetStockInfo(itemDto.SkuId!.Value, itemDto.StockOfficeId!.Value);
        }

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

