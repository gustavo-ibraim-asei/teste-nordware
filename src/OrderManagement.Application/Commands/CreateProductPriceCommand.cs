using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateProductPriceCommand : IRequest<ProductPriceDto>
{
    public CreateProductPriceDto ProductPrice { get; set; } = null!;
}

