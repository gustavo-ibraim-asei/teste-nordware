using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands;

public class UpdateProductCommand : IRequest<ProductDto>
{
    public int Id { get; set; }
    public UpdateProductDto Product { get; set; } = null!;
}



