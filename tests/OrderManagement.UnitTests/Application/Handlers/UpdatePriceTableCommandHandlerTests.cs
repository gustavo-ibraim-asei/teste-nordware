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

public class UpdatePriceTableCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly UpdatePriceTableCommandHandler _handler;
    private const string TenantId = "tenant1";

    public UpdatePriceTableCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        MapperConfiguration mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();
        _handler = new UpdatePriceTableCommandHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePriceTable()
    {
        // Arrange
        PriceTable priceTable = new PriceTable("Tabela Original", "Descrição original", TenantId) { Id = 1 };
        UpdatePriceTableCommand command = new UpdatePriceTableCommand
        {
            Id = 1,
            PriceTable = new UpdatePriceTableDto
            {
                Name = "Tabela Atualizada",
                Description = "Nova descrição",
                IsActive = true
            }
        };

        Mock<IPriceTableRepository> priceTableRepoMock = new Mock<IPriceTableRepository>();
        priceTableRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceTable);

        _unitOfWorkMock.Setup(u => u.PriceTables).Returns(priceTableRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.PriceTables.UpdateAsync(It.IsAny<PriceTable>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        PriceTableDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Tabela Atualizada");
        result.Description.Should().Be("Nova descrição");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentPriceTable_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        UpdatePriceTableCommand command = new UpdatePriceTableCommand
        {
            Id = 999,
            PriceTable = new UpdatePriceTableDto
            {
                Name = "Tabela",
                Description = "Descrição"
            }
        };

        Mock<IPriceTableRepository> priceTableRepoMock = new Mock<IPriceTableRepository>();
        priceTableRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceTable?)null);

        _unitOfWorkMock.Setup(u => u.PriceTables).Returns(priceTableRepoMock.Object);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Tabela de preços com ID 999 não encontrada*");
    }
}

