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

public class CreateStockOfficeCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreateStockOfficeCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateStockOfficeCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateStockOfficeCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateStockOffice()
    {
        // Arrange
        CreateStockOfficeCommand command = new CreateStockOfficeCommand
        {
            StockOffice = new CreateStockOfficeDto
            {
                Name = "Filial São Paulo",
                Code = "SP01"
            }
        };

        _unitOfWorkMock.Setup(u => u.StockOffices.AddAsync(It.IsAny<StockOffice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockOffice so, CancellationToken ct) => so);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        StockOfficeDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Filial São Paulo");
        result.Code.Should().Be("SP01");

        _unitOfWorkMock.Verify(u => u.StockOffices.AddAsync(It.Is<StockOffice>(so => 
            so.Name == "Filial São Paulo" && 
            so.Code == "SP01" && 
            so.TenantId == TenantId), It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

