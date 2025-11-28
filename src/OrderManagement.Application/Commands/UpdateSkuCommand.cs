using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateSkuCommand : IRequest<SkuDto>
{
    public int Id { get; set; }
    public UpdateSkuDto Sku { get; set; } = null!;
}



