using Demo.Architecture.UseCases.Common.Caching;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;

namespace Demo.Architecture.UseCases.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICacheService _cache;

    public CachingBehavior(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
            return await next();

        var cacheKey = cacheableQuery.CacheKey;

        // 1. Try get from cache
        var cached = await _cache.GetAsync<TResponse>(cacheKey);

        if (cached is not null)
            return cached;

        // 2. Execute handler
        var response = await next();

        // 3. Cache ONLY successful results
        if (ShouldCache(response))
        {
            await _cache.SetAsync(cacheKey, response, cacheableQuery.Expiration);
        }

        return response;
    }

    private static bool ShouldCache(TResponse response)
    {
        if (response is IResult result)
        {
            return result.Status == ResultStatus.Ok;
        }

        return true;
    }
}
