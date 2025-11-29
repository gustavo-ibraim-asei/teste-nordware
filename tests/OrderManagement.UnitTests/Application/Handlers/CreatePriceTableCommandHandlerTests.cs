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

public class CreatePriceTableCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly CreatePriceTableCommandHandler _handler;
    private const string TenantId = "tenant1";

    public CreatePriceTableCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _tenantProviderMock.Setup(t => t.GetCurrentTenant()).Returns(TenantId);

        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreatePriceTableCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _tenantProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePriceTable()
    {
        // Arrange
        CreatePriceTableCommand command = new CreatePriceTableCommand
        {
            PriceTable = new CreatePriceTableDto
            {
                Name = "Tabela Padrão",
                Description = "Tabela de preços padrão"
            }
        };

        Mock<IPriceTableRepository> priceTableRepoMock = new Mock<IPriceTableRepository>();
        priceTableRepoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceTable?)null);

        _unitOfWorkMock.Setup(u => u.PriceTables).Returns(priceTableRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.PriceTables.AddAsync(It.IsAny<PriceTable>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceTable pt, CancellationToken _) => pt);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        PriceTableDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Tabela Padrão");
        result.Description.Should().Be("Tabela de preços padrão");
        result.IsActive.Should().BeTrue();

        _unitOfWorkMock.Verify(u => u.PriceTables.AddAsync(It.Is<PriceTable>(pt => 
            pt.Name == "Tabela Padrão" && 
            pt.TenantId == TenantId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        CreatePriceTableCommand command = new CreatePriceTableCommand
        {
            PriceTable = new CreatePriceTableDto
            {
                Name = "Tabela Existente",
                Description = "Descrição"
            }
        };

        PriceTable existing = new PriceTable("Tabela Existente", null, TenantId);
        Mock<IPriceTableRepository> priceTableRepoMock = new Mock<IPriceTableRepository>();
        priceTableRepoMock.Setup(r => r.GetByNameAsync("Tabela Existente", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _unitOfWorkMock.Setup(u => u.PriceTables).Returns(priceTableRepoMock.Object);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já existe uma tabela de preços com o nome*");
    }
}

