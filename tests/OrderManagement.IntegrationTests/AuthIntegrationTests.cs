using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.DTOs;
using OrderManagement.Infrastructure.Data;
using OrderManagement.IntegrationTests.Helpers;
using Xunit;

namespace OrderManagement.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactoryHelper>, IDisposable
{
    private readonly WebApplicationFactoryHelper _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly OrderManagementDbContext _dbContext;

    public AuthIntegrationTests(WebApplicationFactoryHelper factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        // Arrange
        RegisterDto registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            UserName = "newuser",
            Password = "password123",
            TenantId = "test-tenant"
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AuthResultDto? result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
        result.Username.Should().Be("newuser");

        // Verify in database
        Domain.Entities.User? userInDb = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be("newuser@example.com");
        userInDb.TenantId.Should().Be("test-tenant");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange - Create first user
        RegisterDto firstUser = new RegisterDto
        {
            Email = "existing@example.com",
            UserName = "user1",
            Password = "password123",
            TenantId = "test-tenant"
        };

        await _client.PostAsJsonAsync("/api/auth/register", firstUser);

        // Act - Try to register with same email
        RegisterDto duplicateUser = new RegisterDto
        {
            Email = "existing@example.com",
            UserName = "user2",
            Password = "password123",
            TenantId = "test-tenant"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", duplicateUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange - Register user first
        RegisterDto registerDto = new RegisterDto
        {
            Email = "loginuser@example.com",
            UserName = "loginuser",
            Password = "password123",
            TenantId = "test-tenant"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act - Login
        LoginDto loginDto = new LoginDto
        {
            EmailOrUserName = "loginuser",
            Password = "password123",
            TenantId = "test-tenant"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AuthResultDto? result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().NotBeNull();
        result.Email.Should().Be("loginuser@example.com");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange - Register user first
        RegisterDto registerDto = new RegisterDto
        {
            Email = "wrongpass@example.com",
            UserName = "wrongpass",
            Password = "password123",
            TenantId = "test-tenant"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act - Login with wrong password
        LoginDto loginDto = new LoginDto
        {
            EmailOrUserName = "wrongpass",
            Password = "wrongpassword",
            TenantId = "test-tenant"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithWrongTenant_ShouldReturnUnauthorized()
    {
        // Arrange - Register user with tenant-1
        RegisterDto registerDto = new RegisterDto
        {
            Email = "tenantuser@example.com",
            UserName = "tenantuser",
            Password = "password123",
            TenantId = "tenant-1"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act - Login with wrong tenant
        LoginDto loginDto = new LoginDto
        {
            EmailOrUserName = "tenantuser",
            Password = "password123",
            TenantId = "tenant-2" // Wrong tenant
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange - Register and login
        RegisterDto registerDto = new RegisterDto
        {
            Email = "meuser@example.com",
            UserName = "meuser",
            Password = "password123",
            TenantId = "test-tenant"
        };

        HttpResponseMessage registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        AuthResultDto? registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        // Act - Get current user with token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", registerResult!.Token);

        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
    }
}





