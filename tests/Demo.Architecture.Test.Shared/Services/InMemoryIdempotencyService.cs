using Demo.Architecture.UseCases.Common.Idempotency;

namespace Demo.Architecture.Test.Shared.Services;

public class InMemoryIdempotencyService : IIdempotencyService
{
    private readonly Dictionary<string, IdempotencyRecord> _store = new();
    private readonly Dictionary<string, LockEntry> _locks = new();

    private readonly object _lock = new();

    public Task<IdempotencyRecord?> GetAsync(string key)
    {
        lock (_lock)
        {
            _store.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }
    }

    public Task SaveAsync(string key, IdempotencyRecord record, TimeSpan ttl)
    {
        lock (_lock)
        {
            _store[key] = record;
        }

        return Task.CompletedTask;
    }

    // ✅ Acquire with token + TTL
    public Task<(bool Acquired, string Token)> TryAcquireLockAsync(string key, TimeSpan ttl)
    {
        lock (_lock)
        {
            if (_locks.TryGetValue(key, out var existing))
            {
                // Check expiration
                if (existing.Expiry > DateTime.UtcNow)
                {
                    return Task.FromResult((false, string.Empty));
                }

                // Expired → remove
                _locks.Remove(key);
            }

            var token = Guid.NewGuid().ToString();

            _locks[key] = new LockEntry
            {
                Token = token,
                Expiry = DateTime.UtcNow.Add(ttl)
            };

            return Task.FromResult((true, token));
        }
    }

    // ✅ Release with ownership check
    public Task<bool> ReleaseLockAsync(string key, string token)
    {
        lock (_lock)
        {
            if (_locks.TryGetValue(key, out var existing))
            {
                if (existing.Token == token)
                {
                    _locks.Remove(key);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}

public class LockEntry
{
    public string Token { get; set; } = default!;
    public DateTime Expiry { get; set; }
}