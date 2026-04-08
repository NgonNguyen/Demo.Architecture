using Demo.Architecture.UseCases.Common.Idempotency;

namespace Demo.Architecture.Test.Shared.Services;

public class InMemoryIdempotencyService : IIdempotencyService
{
    private readonly Dictionary<string, IdempotencyRecord> _store = new();
    private readonly HashSet<string> _locks = new();

    public Task<IdempotencyRecord?> GetAsync(string key)
    {
        _store.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task SaveAsync(string key, IdempotencyRecord record, TimeSpan ttl)
    {
        _store[key] = record;
        return Task.CompletedTask;
    }

    public Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl)
    {
        lock (_locks)
        {
            if (_locks.Contains(key))
                return Task.FromResult(false);

            _locks.Add(key);
            return Task.FromResult(true);
        }
    }

    public Task ReleaseLockAsync(string key)
    {
        lock (_locks)
        {
            _locks.Remove(key);
        }

        return Task.CompletedTask;
    }
}
