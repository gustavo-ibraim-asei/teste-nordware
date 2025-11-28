using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class CreateProductCommand : IRequest<ProductDto>
{
    public CreateProductDto Product { get; set; } = null!;
}



