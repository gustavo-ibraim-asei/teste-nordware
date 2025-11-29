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

public class CreateSkuCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreateSkuCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateSkuCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateSkuCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSku()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);

        CreateSkuCommand command = new CreateSkuCommand
        {
            Sku = new CreateSkuDto
            {
                ProductId = 1,
                ColorId = 1,
                SizeId = 1
            }
        };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(u => u.Colors.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        _unitOfWorkMock.Setup(u => u.Sizes.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(size);

        _unitOfWorkMock.Setup(u => u.Skus.GetByProductColorSizeAsync(1, 1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sku?)null);

        _unitOfWorkMock.Setup(u => u.Skus.AddAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sku sku, CancellationToken _) => sku);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        SkuDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(1);
        result.ColorId.Should().Be(1);
        result.SizeId.Should().Be(1);
        result.SkuCode.Should().NotBeNullOrEmpty();
        result.Barcode.Should().NotBeNullOrEmpty();

        _unitOfWorkMock.Verify(u => u.Skus.AddAsync(It.IsAny<Sku>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        CreateSkuCommand command = new CreateSkuCommand
        {
            Sku = new CreateSkuDto
            {
                ProductId = 999,
                ColorId = 1,
                SizeId = 1
            }
        };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Produto com ID 999 não encontrado");
    }

    [Fact]
    public async Task Handle_WithExistingSku_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Product product = new Product("Camiseta", "CAM001", "Descrição", TenantId);
        Color color = new Color("Preto", "BLK", TenantId);
        Size size = new Size("M", "M", TenantId);
        Sku existingSku = new Sku(product, color, size, TenantId);

        CreateSkuCommand command = new CreateSkuCommand
        {
            Sku = new CreateSkuDto
            {
                ProductId = 1,
                ColorId = 1,
                SizeId = 1
            }
        };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(u => u.Colors.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(color);

        _unitOfWorkMock.Setup(u => u.Sizes.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(size);

        _unitOfWorkMock.Setup(u => u.Skus.GetByProductColorSizeAsync(1, 1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSku);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SKU já existe para esta combinação de Produto, Cor e Tamanho");
    }
}


