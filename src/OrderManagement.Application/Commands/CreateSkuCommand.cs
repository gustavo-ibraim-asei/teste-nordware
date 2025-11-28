using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateSkuCommand : IRequest<SkuDto>
{
    public CreateSkuDto Sku { get; set; } = null!;
}



