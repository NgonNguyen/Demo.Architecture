using Demo.Architecture.UseCases.Base;
using Demo.Architecture.UseCases.Common.Caching;
using MediatR;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetList;

public record GetListProductsQuery
    : PagedQueryBase, IRequest<Result<AppModels.PagedResult<GetListProductsResponse>>>, ICacheableQuery
{
    public string CachePrefix => "products";

    public string CacheKey => CacheKeyBuilder.Build(CachePrefix, Page, PageSize, SearchTerm, Sort);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
