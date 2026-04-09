namespace Demo.Architecture.UseCases.Common.Caching;

public interface ICacheableQuery
{
    string CachePrefix { get; }
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
