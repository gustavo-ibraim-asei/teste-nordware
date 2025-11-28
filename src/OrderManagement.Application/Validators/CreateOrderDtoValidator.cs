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
            .WithMessage("Customer ID must be greater than zero");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemDtoValidator());

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("Shipping address is required")
            .SetValidator(new AddressDtoValidator());
    }
}

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than zero");

        RuleFor(x => x.ColorId)
            .GreaterThan(0)
            .WithMessage("Color ID must be greater than zero");

        RuleFor(x => x.SizeId)
            .GreaterThan(0)
            .WithMessage("Size ID must be greater than zero");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price cannot be negative");
    }
}

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required");

        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("Number is required");

        RuleFor(x => x.Neighborhood)
            .NotEmpty()
            .WithMessage("Neighborhood is required");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required");

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2)
            .WithMessage("State must be 2 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .Matches(@"^\d{5}-?\d{3}$")
            .WithMessage("ZipCode must be in format 00000-000");
    }
}


