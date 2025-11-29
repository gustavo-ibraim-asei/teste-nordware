using AutoMapper;
using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mappings;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class CreateStockCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreateStockCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateStockCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateStockCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateStock()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku sku = new Sku(product, color, size, TenantId);
        StockOffice stockOffice = new StockOffice("Filial SP", "SP01", TenantId);

        CreateStockCommand command = new CreateStockCommand
        {
            Stock = new CreateStockDto
            {
                SkuId = 1,
                StockOfficeId = 1,
                Quantity = 100
            }
        };

        _unitOfWorkMock.Setup(u => u.Stocks.GetBySkuAndOfficeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);

        Stock createdStock = new Stock(1, 1, 100, TenantId);
        Stock stockWithNavigation = new Stock(1, 1, 100, TenantId);

        _unitOfWorkMock.Setup(u => u.Stocks.AddAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock s, CancellationToken ct) => s)
            .Callback<Stock, CancellationToken>((s, ct) => { createdStock = s; });

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1)
            .Callback(() => { createdStock = new Stock(1, 1, 100, TenantId); });

        _unitOfWorkMock.Setup(u => u.Stocks.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken ct) =>
            {
                var stock = new Stock(1, 1, 100, TenantId);
                return stock;
            });

        // Act
        StockDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SkuId.Should().Be(1);
        result.StockOfficeId.Should().Be(1);
        result.Quantity.Should().Be(100);
        result.AvailableQuantity.Should().Be(100);

        _unitOfWorkMock.Verify(u => u.Stocks.GetBySkuAndOfficeAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Stocks.AddAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Stocks.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Stock existingStock = new Stock(1, 1, 50, TenantId);

        CreateStockCommand command = new CreateStockCommand
        {
            Stock = new CreateStockDto
            {
                SkuId = 1,
                StockOfficeId = 1,
                Quantity = 100
            }
        };

        _unitOfWorkMock.Setup(u => u.Stocks.GetBySkuAndOfficeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStock);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Estoque já existe para este SKU e Filial");
    }
}

