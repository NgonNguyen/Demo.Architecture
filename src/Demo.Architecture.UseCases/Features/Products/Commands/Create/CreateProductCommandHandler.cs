using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;
using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Create;

public class CreateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateProductCommand, Result<Ulid>>
{
    public async Task<Result<Ulid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var result = Product.Create(request.Name, request.Price);

        if (!result.IsSuccess)
            return Result.Invalid(result.ValidationErrors);

        var product = result.Value;

        context.Products.Add(product);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id.Value);
    }
}
