using Demo.Architecture.UseCases.Common.Idempotency;
using StackExchange.Redis;
using System.Text.Json;

namespace Demo.Architecture.Infrastructure.Idempotency;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDatabase _db;

    public RedisIdempotencyService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<IdempotencyRecord?> GetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<IdempotencyRecord>((string)value!);
    }

    public async Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry)
    {
        var lockKey = $"lock:{key}";

        return await _db.StringSetAsync(
            lockKey,
            "1",
            expiry,
            When.NotExists);
    }

    public async Task SaveAsync(string key, IdempotencyRecord record, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(record);

        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task ReleaseLockAsync(string key)
    {
        var lockKey = $"lock:{key}";
        await _db.KeyDeleteAsync(lockKey);
    }
}
