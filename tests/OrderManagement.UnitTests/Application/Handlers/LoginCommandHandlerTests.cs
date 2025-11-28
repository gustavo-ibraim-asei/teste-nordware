using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.API.Services;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Handlers;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;
using Xunit;

namespace OrderManagement.UnitTests.Application.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<JwtService> _jwtServiceMock;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<JwtService>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(), Mock.Of<ILogger<JwtService>>());
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        User user = new User("test@example.com", "testuser", "hashed_password", "test-tenant");
        user.Activate();

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailOrUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns("test_token");

        LoginCommand command = new LoginCommand
        {
            Login = new LoginDto
            {
                EmailOrUserName = "testuser",
                Password = "password123",
                TenantId = "test-tenant"
            }
        };

        // Act
        AuthResultDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test_token");
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailOrUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        LoginCommand command = new LoginCommand
        {
            Login = new LoginDto
            {
                EmailOrUserName = "nonexistent",
                Password = "password123",
                TenantId = "test-tenant"
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        User user = new User("test@example.com", "testuser", "hashed_password", "test-tenant");
        user.Deactivate();

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailOrUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        LoginCommand command = new LoginCommand
        {
            Login = new LoginDto
            {
                EmailOrUserName = "testuser",
                Password = "password123",
                TenantId = "test-tenant"
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Usuário inativo");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        User user = new User("test@example.com", "testuser", "hashed_password", "test-tenant");
        user.Activate();

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailOrUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        LoginCommand command = new LoginCommand
        {
            Login = new LoginDto
            {
                EmailOrUserName = "testuser",
                Password = "wrongpassword",
                TenantId = "test-tenant"
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas");
    }

    [Fact]
    public async Task Handle_WithWrongTenant_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        User user = new User("test@example.com", "testuser", "hashed_password", "tenant-1");
        user.Activate();

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailOrUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        LoginCommand command = new LoginCommand
        {
            Login = new LoginDto
            {
                EmailOrUserName = "testuser",
                Password = "password123",
                TenantId = "tenant-2" // Wrong tenant
            }
        };

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Tenant incorreto");
    }
}


