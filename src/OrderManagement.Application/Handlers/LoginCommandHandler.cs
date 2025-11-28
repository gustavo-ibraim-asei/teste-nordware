using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ILogger<LoginCommandHandler> logger, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _jwtService = jwtService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuário por email ou username
        Domain.Entities.User? user = await _unitOfWork.Users.GetByEmailOrUserNameAsync(request.Login.EmailOrUserName, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Tentativa de login com credenciais inválidas: {EmailOrUserName}", request.Login.EmailOrUserName);
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Tentativa de login com usuário inativo: {UserName}", user.UserName);
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Verificar senha
        bool passwordValid = _passwordHasher.VerifyPassword(request.Login.Password, user.PasswordHash);
        if (!passwordValid)
        {
            _logger.LogWarning("Senha inválida para usuário: {UserName}", user.UserName);
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        // Verificar tenant se fornecido
        string tenantId = request.Login.TenantId ?? user.TenantId;
        if (user.TenantId != tenantId)
        {
            _logger.LogWarning("Tentativa de login com tenant incorreto: {UserName}, tenant esperado: {ExpectedTenant}, fornecido: {ProvidedTenant}", user.UserName, user.TenantId, tenantId);
            throw new UnauthorizedAccessException("Tenant incorreto");
        }

        // Atualizar último login
        user.UpdateLastLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Buscar roles do usuário
        List<string> roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToList();

        if (!roles.Any())
            roles.Add("User");

        // Gerar token JWT
        string token = _jwtService.GenerateToken(user.UserName, user.TenantId, roles);

        _logger.LogInformation("Usuário {UserName} autenticado com sucesso", user.UserName);

        return new AuthResultDto
        {
            Token = token,
            Username = user.UserName,
            Email = user.Email,
            TenantId = user.TenantId,
            Roles = roles,
            ExpiresIn = 28800 // 8 horas em segundos
        };
    }
}

