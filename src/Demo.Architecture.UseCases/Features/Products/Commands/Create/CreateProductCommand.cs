using Demo.Architecture.UseCases.Common.Attributes;
using Demo.Architecture.UseCases.Common.Caching;
using Demo.Architecture.UseCases.Common.Messaging.Commands;
using NUlid;
using System.ComponentModel;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Create;

[RequireIdempotency]
public record CreateProductCommand(
    [property: Description("Name of product.")]
    string Name,
    [property: Description("Price of product.")]
    decimal Price
) : BaseCommand<Result<Ulid>>, ICacheInvalidationCommand
{
    public IEnumerable<string> CacheKeys =>
        new[]
        {
            "products:*"
        };
}
