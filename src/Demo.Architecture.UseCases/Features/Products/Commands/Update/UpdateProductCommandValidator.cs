using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Rules;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Update;

public class UpdateProductCommandValidator
    : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IProductUniquenessChecker checker)
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage(ProductErrors.NameRequired.Message)
                .WithErrorCode(ProductErrors.NameRequired.Code)
            .MaximumLength(200);

        RuleFor(v => v)
            .MustAsync(async (cmd, ct) =>
                await checker.IsNameUnique(cmd.Name, cmd.Id, ct))
            .WithMessage(ProductErrors.DuplicatedName.Message)
            .WithErrorCode(ProductErrors.DuplicatedName.Code);

        RuleFor(x => x.Price)
            .GreaterThan(0)
                .WithMessage(ProductErrors.PriceInvalid.Message)
                .WithErrorCode(ProductErrors.PriceInvalid.Code);
    }
}
