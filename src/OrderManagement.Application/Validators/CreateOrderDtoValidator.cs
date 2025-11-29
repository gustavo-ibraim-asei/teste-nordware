using FluentValidation;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.DTOs.ValueObjects;

namespace OrderManagement.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("O ID do cliente deve ser maior que zero");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("O pedido deve ter pelo menos um item");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemDtoValidator());

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("O endereço de entrega é obrigatório")
            .SetValidator(new AddressDtoValidator());
    }
}

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("O ID do produto deve ser maior que zero");

        RuleFor(x => x.ColorId)
            .GreaterThan(0)
            .WithMessage("O ID da cor deve ser maior que zero");

        RuleFor(x => x.SizeId)
            .GreaterThan(0)
            .WithMessage("O ID do tamanho deve ser maior que zero");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("O nome do produto é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("A quantidade deve ser maior que zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O preço unitário não pode ser negativo");
    }
}

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("A rua é obrigatória");

        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("O número é obrigatório");

        RuleFor(x => x.Neighborhood)
            .NotEmpty()
            .WithMessage("O bairro é obrigatório");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("A cidade é obrigatória");

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2)
            .WithMessage("O estado deve ter 2 caracteres");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .Matches(@"^\d{5}-?\d{3}$")
            .WithMessage("O CEP deve estar no formato 00000-000");
    }
}


