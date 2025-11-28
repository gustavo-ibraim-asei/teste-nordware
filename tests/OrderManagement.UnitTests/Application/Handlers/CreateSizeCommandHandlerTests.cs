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

public class CreateSizeCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreateSizeCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreateSizeCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreateSizeCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSize()
    {
        // Arrange
        CreateSizeCommand command = new CreateSizeCommand
        {
            Size = new CreateSizeDto
            {
                Name = "M",
                Code = "M"
            }
        };

        _unitOfWorkMock.Setup(u => u.Sizes.AddAsync(It.IsAny<Size>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Size s, CancellationToken ct) => s);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        SizeDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("M");
        result.Code.Should().Be("M");

        _unitOfWorkMock.Verify(u => u.Sizes.AddAsync(It.Is<Size>(s => 
            s.Name == "M" && 
            s.Code == "M" && 
            s.TenantId == TenantId), It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

