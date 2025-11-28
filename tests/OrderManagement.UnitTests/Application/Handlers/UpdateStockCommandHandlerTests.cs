using AutoMapper;
using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Mappings;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class UpdateStockCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly UpdateStockCommandHandler _handler;
    private const string TenantId = "tenant1";

    public UpdateStockCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new UpdateStockCommandHandler(
            _unitOfWorkMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateStock()
    {
        // Arrange
        Stock existingStock = new Stock(1, 1, 100, TenantId) { Id = 1 };

        UpdateStockCommand command = new UpdateStockCommand
        {
            Id = 1,
            Stock = new UpdateStockDto
            {
                Quantity = 150
            }
        };

        _unitOfWorkMock.Setup(u => u.Stocks.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStock);

        _unitOfWorkMock.Setup(u => u.Stocks.UpdateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.Stocks.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken ct) => 
            {
                var stock = new Stock(1, 1, 150, TenantId) { Id = id };
                return stock;
            });

        // Act
        StockDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(150);
        result.AvailableQuantity.Should().Be(150);

        _unitOfWorkMock.Verify(u => u.Stocks.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.Stocks.UpdateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentStock_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        UpdateStockCommand command = new UpdateStockCommand
        {
            Id = 999,
            Stock = new UpdateStockDto
            {
                Quantity = 150
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

