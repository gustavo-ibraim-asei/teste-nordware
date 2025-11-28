using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.API.Attributes;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
    {
        try
        {
            RegisterUserCommand command = new RegisterUserCommand { Register = registerDto };
            AuthResultDto result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        try
        {
            LoginCommand command = new LoginCommand { Login = loginDto };
            AuthResultDto result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint protegido para testar autenticação JWT
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [JwtRequired]
    public IActionResult GetCurrentUser()
    {
        string? username = User.Identity?.Name;
        string? tenantId = User.FindFirst("tenant_id")?.Value;
        List<string> roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            Username = username,
            TenantId = tenantId,
            Roles = roles
        });
    }
}

