using Demo.Architecture.UseCases.Common.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Demo.Architecture.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>((byte[])value!, Shared.Serialization.JsonSerializerDefaults.Options);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(
            key: key,
            value: JsonSerializer.Serialize(value, Shared.Serialization.JsonSerializerDefaults.Options),
            expiry: expiry,
            when: When.Always,
            flags: CommandFlags.None
        );
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        var keys = server.Keys(pattern: pattern);

        foreach (var key in keys)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}
