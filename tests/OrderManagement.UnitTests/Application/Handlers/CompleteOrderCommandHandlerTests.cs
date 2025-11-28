using AutoMapper;
using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Mappings;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class CompleteOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<IShippingCalculationService> _shippingServiceMock;
    private readonly CompleteOrderCommandHandler _handler;

    public CompleteOrderCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _shippingServiceMock = new Mock<IShippingCalculationService>();

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CompleteOrderCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _eventDispatcherMock.Object,
            _shippingServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCompleteOrder()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 2, 29.99m)
        };
        Order order = new Order(1, address, items, "test-tenant");

        List<ShippingOption> shippingOptions = new List<ShippingOption>
        {
            new ShippingOption(1, "Correios", 1, "Padrão", 15.00m, 2)
        };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _shippingServiceMock.Setup(s => s.CalculateShippingOptionsAsync(
            It.IsAny<string>(), 
            It.IsAny<decimal>(), 
            It.IsAny<decimal>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(shippingOptions);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        CompleteOrderCommand command = new CompleteOrderCommand
        {
            OrderId = 1,
            ShippingInfo = new CompleteOrderDto
            {
                CarrierId = 1,
                ShippingTypeId = 1
            }
        };

        // Act
        OrderDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Confirmed);
        result.CarrierId.Should().Be(1);
        result.ShippingTypeId.Should().Be(1);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        CompleteOrderCommand command = new CompleteOrderCommand
        {
            OrderId = 999,
            ShippingInfo = new CompleteOrderDto
            {
                CarrierId = 1,
                ShippingTypeId = 1
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Pedido com ID 999 não encontrado");
    }

    [Fact]
    public async Task Handle_WithNonPendingOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 1, 29.99m)
        };
        Order order = new Order(1, address, items, "test-tenant");
        order.UpdateStatus(OrderStatus.Confirmed);

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        CompleteOrderCommand command = new CompleteOrderCommand
        {
            OrderId = 1,
            ShippingInfo = new CompleteOrderDto
            {
                CarrierId = 1,
                ShippingTypeId = 1
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Apenas pedidos pendentes podem ser finalizados");
    }

    [Fact]
    public async Task Handle_WithInvalidShippingOption_ShouldThrowArgumentException()
    {
        // Arrange
        Address address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01310-100");
        List<OrderItem> items = new List<OrderItem>
        {
            new OrderItem(1, "Product 1", 1, 29.99m)
        };
        Order order = new Order(1, address, items, "test-tenant");

        List<ShippingOption> shippingOptions = new List<ShippingOption>
        {
            new ShippingOption(1, "Correios", 1, "Padrão", 15.00m, 2)
        };

        _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _shippingServiceMock.Setup(s => s.CalculateShippingOptionsAsync(
            It.IsAny<string>(), 
            It.IsAny<decimal>(), 
            It.IsAny<decimal>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(shippingOptions);

        CompleteOrderCommand command = new CompleteOrderCommand
        {
            OrderId = 1,
            ShippingInfo = new CompleteOrderDto
            {
                CarrierId = 999, // Invalid carrier
                ShippingTypeId = 999 // Invalid type
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Opção de frete selecionada não encontrada ou não disponível");
    }
}





