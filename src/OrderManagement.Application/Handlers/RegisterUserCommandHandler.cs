using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ILogger<RegisterUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Verificar se email já existe
        bool emailExists = await _unitOfWork.Users.ExistsByEmailAsync(request.Register.Email, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Tentativa de registro com email já existente: {Email}", request.Register.Email);
            throw new InvalidOperationException("Email já está em uso");
        }

        // Verificar se username já existe
        bool userNameExists = await _unitOfWork.Users.ExistsByUserNameAsync(request.Register.UserName, cancellationToken);
        if (userNameExists)
        {
            _logger.LogWarning("Tentativa de registro com nome de usuário já existente: {UserName}", request.Register.UserName);
            throw new InvalidOperationException("Nome de usuário já está em uso");
        }

        // Criar hash da senha
        string passwordHash = _passwordHasher.HashPassword(request.Register.Password);

        // Criar usuário
        Domain.Entities.User user = new Domain.Entities.User(
            request.Register.Email,
            request.Register.UserName,
            passwordHash,
            request.Register.TenantId);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuário {UserName} registrado com sucesso para tenant {TenantId}", request.Register.UserName, request.Register.TenantId);

        // Retornar resultado (sem token, precisa fazer login)
        return new AuthResultDto
        {
            Username = user.UserName,
            Email = user.Email,
            TenantId = user.TenantId,
            Roles = new List<string> { "User" },
            ExpiresIn = 0
        };
    }
}

