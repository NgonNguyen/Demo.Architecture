using Demo.Architecture.UseCases.Common.Caching;
using MediatR;
using NUlid;
using System.ComponentModel;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Create;

public record CreateProductCommand(
    [property: Description("Name of product.")]
    string Name,
    [property: Description("Price of product.")]
    decimal Price
) : IRequest<Result<Ulid>>, ICacheInvalidationCommand
{
    public IEnumerable<string> CacheKeys =>
        new[]
        {
            "products:*"
        };
}
