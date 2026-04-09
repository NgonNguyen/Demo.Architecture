using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Caching;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetById;

public sealed record GetProductByIdQuery(
    [FromRoute(Name = "id")]
    [Description("An identifier for a product.")]
    ProductId Id
) : IRequest<Result<GetProductByIdResponse>>, ICacheableQuery
{
    public string CachePrefix => "product";

    public string CacheKey => $"{CachePrefix}:{Id}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
