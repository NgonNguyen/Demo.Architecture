using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Update;

public class UpdateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.Id);
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
            return Result.NotFound(ProductErrors.NotFound.Code);

        var updateResult = product.Update(command.Name, command.Price);

        if (!updateResult.IsSuccess)
            return updateResult;

        return Result.Success();
    }
}
