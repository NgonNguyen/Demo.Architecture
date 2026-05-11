using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Common.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetList;

public class GetListProductsHandler(IReadOnlyApplicationDbContext context,
    ILogger<GetListProductsHandler> logger)
    : IRequestHandler<GetListProductsQuery, Result<AppModels.PagedResult<GetListProductsResponse>>>
{
    public async Task<Result<AppModels.PagedResult<GetListProductsResponse>>> Handle(
        GetListProductsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{Request}", request);
        var spec = new GetListProductsSpecification(request.SearchTerm, request.Sort);

        var query = context.Products
            .ApplySpecification(spec);

        var total = await query.CountAsync(cancellationToken);

        var data = await query.Skip(request.Skip).Take(request.PageSize)
            .Select(x => new GetListProductsResponse(
                x.Id.Value,
                x.Name,
                x.Price.Value
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(new AppModels.PagedResult<GetListProductsResponse>(
            data,
            total,
            request.Page,
            request.PageSize));
    }
}
