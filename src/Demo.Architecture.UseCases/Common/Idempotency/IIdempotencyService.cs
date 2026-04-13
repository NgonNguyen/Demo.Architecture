namespace Demo.Architecture.UseCases.Common.Idempotency;

public interface IIdempotencyService
{
    Task<IdempotencyRecord?> GetAsync(string key);

    Task<(bool Acquired, string Token)> TryAcquireLockAsync(string key, TimeSpan expiry);

    Task SaveAsync(string key, IdempotencyRecord record, TimeSpan ttl);

    Task<bool> ReleaseLockAsync(string key, string token);
}
