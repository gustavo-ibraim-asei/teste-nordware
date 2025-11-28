using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class RegisterUserCommand : IRequest<AuthResultDto>
{
    public RegisterDto Register { get; set; } = null!;
}


