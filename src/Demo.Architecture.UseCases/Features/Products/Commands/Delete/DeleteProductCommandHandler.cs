using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Delete;

public class DeleteProductCommandHandler(IApplicationDbContext context)
     : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.Id);

        var product = await context.Products
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null)
            return Result.NotFound(ProductErrors.NotFound.Code);

        product.Deactivate();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
