using AutoMapper;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mappings;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly CreateProductCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();

        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateProductCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        CreateProductCommand command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Name = "Camiseta Básica",
                Code = "CAM001",
                Description = "Camiseta de algodão"
            }
        };

        _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        ProductDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Camiseta Básica");
        result.Code.Should().Be("CAM001");
        result.Description.Should().Be("Camiseta de algodão");

        _unitOfWorkMock.Verify(u => u.Products.AddAsync(It.Is<Product>(p => 
            p.Name == "Camiseta Básica" && 
            p.Code == "CAM001" && 
            p.TenantId == TenantId), It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldInvalidateCache()
    {
        // Arrange
        CreateProductCommand command = new CreateProductCommand
        {
            Product = new CreateProductDto
            {
                Name = "Camiseta Básica",
                Code = "CAM001",
                Description = "Camiseta de algodão"
            }
        };

        _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync("products:all", It.IsAny<CancellationToken>()), Times.Once);
    }
}


