using Demo.Architecture.UseCases.Common.Caching;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;

namespace Demo.Architecture.UseCases.Common.Behaviors;

public class CacheInvalidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICacheService _cache;

    public CacheInvalidationBehavior(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is not ICacheInvalidationCommand command)
            return response;

        // Only invalidate on success
        if (response is IResult result && result.Status != ResultStatus.Ok)
            return response;

        foreach (var key in command.CacheKeys)
        {
            if (key.Contains("*"))
                await _cache.RemoveByPatternAsync(key);
            else
                await _cache.RemoveAsync(key);
        }

        return response;
    }
}
