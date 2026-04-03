namespace Demo.Architecture.UseCases.Common.Caching;

public interface ICacheInvalidationCommand
{
    IEnumerable<string> CacheKeys { get; }
}
