using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Common.Specifications;
using Demo.Architecture.UseCases.Features.Products.Errors;
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
            .ApplySpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result<GetProductByIdResponse>.NotFound(new[] { ProductErrors.NotFound });
        }

        var response = GetProductByIdResponse.FromEntity(product);

        return Result.Success(response);
    }
}
