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

public class CreateProductPriceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreateProductPriceCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateProductPriceCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateProductPriceCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductPrice()
    {
        // Arrange
        Product product = new Product("Produto Teste", "PROD001", null, TenantId);
        PriceTable priceTable = new PriceTable("Tabela Teste", null, TenantId);

        CreateProductPriceCommand command = new CreateProductPriceCommand
        {
            ProductPrice = new CreateProductPriceDto
            {
                ProductId = product.Id,
                PriceTableId = priceTable.Id,
                UnitPrice = 99.99m
            }
        };

        Mock<IProductPriceRepository> productPriceRepoMock = new Mock<IProductPriceRepository>();
        productPriceRepoMock.Setup(r => r.GetByProductAndPriceTableAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPrice?)null);

        Mock<IRepository<Product>> productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Mock<IPriceTableRepository> priceTableRepoMock = new Mock<IPriceTableRepository>();
        priceTableRepoMock.Setup(r => r.GetByIdAsync(priceTable.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceTable);

        _unitOfWorkMock.Setup(u => u.ProductPrices).Returns(productPriceRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.PriceTables).Returns(priceTableRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ProductPrices.AddAsync(It.IsAny<ProductPrice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        ProductPrice created = new ProductPrice(product, priceTable, 99.99m, TenantId);
        productPriceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        ProductPriceDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UnitPrice.Should().Be(99.99m);

        _unitOfWorkMock.Verify(u => u.ProductPrices.AddAsync(It.Is<ProductPrice>(pp => 
            pp.ProductId == product.Id && 
            pp.PriceTableId == priceTable.Id &&
            pp.UnitPrice == 99.99m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateProductPrice_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Product product = new Product("Produto Teste", "PROD001", null, TenantId);
        PriceTable priceTable = new PriceTable("Tabela Teste", null, TenantId);
        ProductPrice existing = new ProductPrice(product, priceTable, 50m, TenantId);

        CreateProductPriceCommand command = new CreateProductPriceCommand
        {
            ProductPrice = new CreateProductPriceDto
            {
                ProductId = product.Id,
                PriceTableId = priceTable.Id,
                UnitPrice = 99.99m
            }
        };

        Mock<IProductPriceRepository> productPriceRepoMock = new Mock<IProductPriceRepository>();
        productPriceRepoMock.Setup(r => r.GetByProductAndPriceTableAsync(
            product.Id, priceTable.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _unitOfWorkMock.Setup(u => u.ProductPrices).Returns(productPriceRepoMock.Object);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já existe um preço para este produto*");
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        CreateProductPriceCommand command = new CreateProductPriceCommand
        {
            ProductPrice = new CreateProductPriceDto
            {
                ProductId = 999,
                PriceTableId = 1,
                UnitPrice = 99.99m
            }
        };

        Mock<IProductPriceRepository> productPriceRepoMock = new Mock<IProductPriceRepository>();
        productPriceRepoMock.Setup(r => r.GetByProductAndPriceTableAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPrice?)null);

        Mock<IRepository<Product>> productRepoMock = new Mock<IRepository<Product>>();
        productRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock.Setup(u => u.ProductPrices).Returns(productPriceRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(productRepoMock.Object);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Produto com ID 999 não encontrado*");
    }
}

