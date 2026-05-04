using Demo.Architecture.UseCases.Common.Attributes;
using Demo.Architecture.UseCases.Common.Caching;
using Demo.Architecture.UseCases.Common.Messaging.Commands;
using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Delete;

[RequireIdempotency]
public record DeleteProductCommand(Ulid Id) : BaseCommand<Result>, ICacheInvalidationCommand
{
    public IEnumerable<string> CacheKeys =>
        new[]
        {
            $"product:{Id}",
            "products:*"
        };
}
