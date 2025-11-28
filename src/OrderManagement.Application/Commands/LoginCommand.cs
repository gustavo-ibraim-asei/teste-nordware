using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class LoginCommand : IRequest<AuthResultDto>
{
    public LoginDto Login { get; set; } = null!;
}


