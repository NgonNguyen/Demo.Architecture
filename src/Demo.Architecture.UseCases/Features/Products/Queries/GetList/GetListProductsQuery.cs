using Demo.Architecture.UseCases.Base;
using Demo.Architecture.UseCases.Common.Caching;
using MediatR;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetList;

public record GetListProductsQuery
    : PagedQueryBase, IRequest<Result<AppModels.PagedResult<GetListProductsResponse>>>, ICacheableQuery
{
    public string CacheKey => $"products:{Page}:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
