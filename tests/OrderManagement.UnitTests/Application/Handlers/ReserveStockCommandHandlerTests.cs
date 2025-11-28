using AutoMapper;
using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Mappings;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class ReserveStockCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<IStockService> _stockServiceMock;
    private readonly ReserveStockCommandHandler _handler;
    private const string TenantId = "tenant1";

    public ReserveStockCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _stockServiceMock = new Mock<IStockService>();

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new ReserveStockCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _stockServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReserveStock()
    {
        // Arrange
        Stock existingStock = new Stock(1, 1, 100, TenantId) { Id = 1 };

        ReserveStockCommand command = new ReserveStockCommand
        {
            StockId = 1,
            Reserve = new ReserveStockDto
            {
                Quantity = 10
            }
        };

        _unitOfWorkMock.Setup(u => u.Stocks.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStock);

        _stockServiceMock.Setup(s => s.ReserveStockAsync(1, 1, 10, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Stock reservedStock = new Stock(1, 1, 100, TenantId) { Id = 1 };
        reservedStock.Reserve(10);

        _unitOfWorkMock.Setup(u => u.Stocks.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken ct) => reservedStock);

        // Act
        StockDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(100);
        result.Reserved.Should().Be(10);
        result.AvailableQuantity.Should().Be(90);

        _unitOfWorkMock.Verify(u => u.Stocks.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _stockServiceMock.Verify(s => s.ReserveStockAsync(1, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentStock_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        ReserveStockCommand command = new ReserveStockCommand
        {
            StockId = 999,
            Reserve = new ReserveStockDto
            {
                Quantity = 10
            }
        };

        _unitOfWorkMock.Setup(u => u.Stocks.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Estoque com ID 999 não encontrado");
    }
}

