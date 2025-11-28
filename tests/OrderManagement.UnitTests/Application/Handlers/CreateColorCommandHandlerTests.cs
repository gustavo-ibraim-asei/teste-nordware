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

public class CreateColorCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreateColorCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateColorCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateColorCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateColor()
    {
        // Arrange
        CreateColorCommand command = new CreateColorCommand
        {
            Color = new CreateColorDto
            {
                Name = "Preto",
                Code = "BLK"
            }
        };

        _unitOfWorkMock.Setup(u => u.Colors.AddAsync(It.IsAny<Color>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Color c, CancellationToken ct) => c);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        ColorDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Preto");
        result.Code.Should().Be("BLK");

        _unitOfWorkMock.Verify(u => u.Colors.AddAsync(It.Is<Color>(c => 
            c.Name == "Preto" && 
            c.Code == "BLK" && 
            c.TenantId == TenantId), It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

