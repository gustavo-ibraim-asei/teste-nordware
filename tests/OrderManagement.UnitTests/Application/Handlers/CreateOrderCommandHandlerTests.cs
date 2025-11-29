using AutoMapper;
using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mappings;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Domain.ValueObjects;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<IOrderFactory> _orderFactoryMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IShippingCalculationService> _shippingServiceMock;
    private readonly Mock<IStockService> _stockServiceMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _orderFactoryMock = new Mock<IOrderFactory>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _shippingServiceMock = new Mock<IShippingCalculationService>();
        _stockServiceMock = new Mock<IStockService>();

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateOrderCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _eventDispatcherMock.Object,
            _orderFactoryMock.Object,
            _tenantProviderMock.Object,
            _shippingServiceMock.Object,
            _stockServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        // Arrange
        CreateOrderCommand command = new CreateOrderCommand
        {
            Order = new CreateOrderDto
            {
                CustomerId = 1,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto
                    {
                        ProductId = 1,
                        ColorId = 2,
                        SizeId = 3,
                        SkuId = 1,
                        ProductName = "Product 1",
                        Quantity = 2,
                        UnitPrice = 29.99m
                    }
                },
                ShippingAddress = new OrderManagement.Application.DTOs.ValueObjects.AddressDto
                {
                    Street = "Rua Teste",
                    Number = "123",
                    Neighborhood = "Centro",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01310-100"
                }
            }
        };

        _unitOfWorkMock.Setup(u => u.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        OrderDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(1);
        result.Items.Should().HaveCount(1);
        _unitOfWorkMock.Verify(u => u.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

