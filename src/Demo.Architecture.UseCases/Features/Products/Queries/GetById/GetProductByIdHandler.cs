using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Common.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetById;

public class GetProductByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    public async Task<Result<GetProductByIdResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new GetProductByIdSpecification(request.Id);

        var product = await context.Products
            .Where(x => x.IsActive)
            .ApplySpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result<GetProductByIdResponse>.NotFound(ProductErrors.NotFound.Code);
        }

        var response = GetProductByIdResponse.FromEntity(product);

        return Result.Success(response);
    }
}
