using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateProductPriceCommand : IRequest<ProductPriceDto>
{
    public int Id { get; set; }
    public UpdateProductPriceDto ProductPrice { get; set; } = null!;
}

