using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Features.Products.Rules;
using FluentValidation;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Create;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IProductUniquenessChecker checker)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage(ProductErrors.NameRequired.Message)
                .WithErrorCode(ProductErrors.NameRequired.Code)
            .MaximumLength(200);

        RuleFor(v => v.Name)
            .MustAsync(async (name, ct) =>
                await checker.IsNameUnique(name, ct))
            .WithMessage(ProductErrors.DuplicatedName.Message)
            .WithErrorCode(ProductErrors.DuplicatedName.Code);

        RuleFor(x => x.Price)
            .GreaterThan(0)
                .WithMessage(ProductErrors.PriceInvalid.Message)
                .WithErrorCode(ProductErrors.PriceInvalid.Code);
    }
}
