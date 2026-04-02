namespace Demo.Architecture.UseCases.Common.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
