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

    public async Task<(bool Acquired, string Token)> TryAcquireLockAsync(string key, TimeSpan expiry)
    {
        var lockKey = $"lock:{key}";
        var token = Guid.NewGuid().ToString();

        var acquired = await _db.StringSetAsync(
            lockKey,
            token,
            expiry,
            When.NotExists);

        return (acquired, token);
    }

    public async Task SaveAsync(string key, IdempotencyRecord record, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(record);

        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task<bool> ReleaseLockAsync(string key, string token)
    {
        var lockKey = $"lock:{key}";

        var script = @"
        if redis.call('get', KEYS[1]) == ARGV[1]
        then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

        var result = (long)await _db.ScriptEvaluateAsync(
            script,
            new RedisKey[] { lockKey },
            new RedisValue[] { token });

        return result == 1;
    }
}
