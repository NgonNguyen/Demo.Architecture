using Demo.Architecture.UseCases.Common.Attributes;
using Demo.Architecture.UseCases.Common.Caching;
using Demo.Architecture.UseCases.Common.Messaging.Commands;
using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Update;

[RequireIdempotency]
public record UpdateProductCommand(
    Ulid Id,  
    string Name,
    decimal Price
) : BaseCommand<Result>, ICacheInvalidationCommand
{
    public IEnumerable<string> CacheKeys =>
        new[]
        {
            $"product:{Id}",
            "products:*"
        };
}
