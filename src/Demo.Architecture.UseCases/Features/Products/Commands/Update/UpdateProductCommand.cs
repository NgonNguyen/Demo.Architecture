using Demo.Architecture.UseCases.Common.Attributes;
using Demo.Architecture.UseCases.Common.Caching;
using MediatR;
using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Update;

[RequireIdempotency]
public record UpdateProductCommand(
    Ulid Id,  
    string Name,
    decimal Price
) : IRequest<Result>, ICacheInvalidationCommand
{
    public IEnumerable<string> CacheKeys =>
        new[]
        {
            $"product:{Id}",
            "products:*"
        };
}
