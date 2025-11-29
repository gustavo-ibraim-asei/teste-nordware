using FluentValidation;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Validators;

public class CreateStockOfficeDtoValidator : AbstractValidator<CreateStockOfficeDto>
{
    public CreateStockOfficeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da filial é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome da filial deve ter no máximo 200 caracteres");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Código da filial deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}

public class UpdateStockOfficeDtoValidator : AbstractValidator<UpdateStockOfficeDto>
{
    public UpdateStockOfficeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da filial é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome da filial deve ter no máximo 200 caracteres");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Código da filial deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}

public class CreateColorDtoValidator : AbstractValidator<CreateColorDto>
{
    public CreateColorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da cor é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da cor deve ter no máximo 100 caracteres");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Código da cor deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}

public class UpdateColorDtoValidator : AbstractValidator<UpdateColorDto>
{
    public UpdateColorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da cor é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da cor deve ter no máximo 100 caracteres");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Código da cor deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}

public class CreateSizeDtoValidator : AbstractValidator<CreateSizeDto>
{
    public CreateSizeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do tamanho é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome do tamanho deve ter no máximo 100 caracteres");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Código do tamanho deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}

public class UpdateSizeDtoValidator : AbstractValidator<UpdateSizeDto>
{
    public UpdateSizeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do tamanho é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome do tamanho deve ter no máximo 100 caracteres");

        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Código do tamanho deve ter no máximo 50 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}

public class CreateSkuDtoValidator : AbstractValidator<CreateSkuDto>
{
    public CreateSkuDtoValidator()
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
    }
}

public class UpdateSkuDtoValidator : AbstractValidator<UpdateSkuDto>
{
    public UpdateSkuDtoValidator()
    {
        RuleFor(x => x.Barcode)
            .Must(barcode => string.IsNullOrWhiteSpace(barcode) || OrderManagement.Domain.Helpers.EanGenerator.IsValidEan(barcode))
            .WithMessage("Barcode deve estar no formato EAN-8 ou EAN-13 válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Barcode));
    }
}

public class CreateStockDtoValidator : AbstractValidator<CreateStockDto>
{
    public CreateStockDtoValidator()
    {
        RuleFor(x => x.SkuId)
            .GreaterThan(0)
            .WithMessage("O ID do SKU deve ser maior que zero");

        RuleFor(x => x.StockOfficeId)
            .GreaterThan(0)
            .WithMessage("O ID da filial deve ser maior que zero");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantidade não pode ser negativa");
    }
}

public class UpdateStockDtoValidator : AbstractValidator<UpdateStockDto>
{
    public UpdateStockDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantidade não pode ser negativa");
    }
}

public class ReserveStockDtoValidator : AbstractValidator<ReserveStockDto>
{
    public ReserveStockDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade para reserva deve ser maior que zero");
    }
}

public class DecreaseStockDtoValidator : AbstractValidator<DecreaseStockDto>
{
    public DecreaseStockDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade para baixa deve ser maior que zero");
    }
}

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do produto é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome do produto deve ter no máximo 200 caracteres");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Código do produto é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código do produto deve ter no máximo 50 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Descrição do produto deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do produto é obrigatório")
            .MaximumLength(200)
            .WithMessage("Nome do produto deve ter no máximo 200 caracteres");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Código do produto é obrigatório")
            .MaximumLength(50)
            .WithMessage("Código do produto deve ter no máximo 50 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Descrição do produto deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

