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

public class UpdateProductPriceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly UpdateProductPriceCommandHandler _handler;
    private const string TenantId = "tenant1";

    public UpdateProductPriceCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();
        _handler = new UpdateProductPriceCommandHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProductPrice()
    {
        // Arrange
        Product product = new Product("Produto Teste", "PROD001", null, TenantId) { Id = 1 };
        PriceTable priceTable = new PriceTable("Tabela Teste", null, TenantId) { Id = 1 };
        ProductPrice productPrice = new ProductPrice(product, priceTable, 99.99m, TenantId) { Id = 1 };

        UpdateProductPriceCommand command = new UpdateProductPriceCommand
        {
            Id = 1,
            ProductPrice = new UpdateProductPriceDto
            {
                UnitPrice = 149.99m
            }
        };

        Mock<IProductPriceRepository> productPriceRepoMock = new Mock<IProductPriceRepository>();
        productPriceRepoMock.SetupSequence(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productPrice)
            .ReturnsAsync((ProductPrice?)null);

        _unitOfWorkMock.Setup(u => u.ProductPrices).Returns(productPriceRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductPrices.UpdateAsync(It.IsAny<ProductPrice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        ProductPriceDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UnitPrice.Should().Be(149.99m);
    }

    [Fact]
    public async Task Handle_WithNonExistentProductPrice_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        UpdateProductPriceCommand command = new UpdateProductPriceCommand
        {
            Id = 999,
            ProductPrice = new UpdateProductPriceDto
            {
                UnitPrice = 99.99m
            }
        };

        Mock<IProductPriceRepository> productPriceRepoMock = new Mock<IProductPriceRepository>();
        productPriceRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPrice?)null);

        _unitOfWorkMock.Setup(u => u.ProductPrices).Returns(productPriceRepoMock.Object);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Preço do produto com ID 999 não encontrado*");
    }
}

