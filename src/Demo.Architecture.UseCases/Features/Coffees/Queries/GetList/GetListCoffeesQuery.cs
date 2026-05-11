using Demo.Architecture.UseCases.Base;
using Demo.Architecture.UseCases.Common.Caching;
using MediatR;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Coffees.Queries.GetList;

public record GetListCoffeesQuery 
    : PagedQueryBase, IRequest<Result<AppModels.PagedResult<GetListCoffeesResponse>>> //, ICacheableQuery
{
    //public string CachePrefix => "coffees";

    //public string CacheKey => CacheKeyBuilder.Build(CachePrefix, Page, PageSize, SearchTerm, Sort);

    //public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
