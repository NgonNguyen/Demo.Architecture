using Demo.Architecture.UseCases.Common.Interfaces;
using System.Text.Json;
using StackExchange.Redis;
using Demo.Architecture.Infrastructure.Serialization;

namespace Demo.Architecture.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new UlidJsonConverter() }
    };

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>((byte[])value!, _options);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(
            key: key,
            value: JsonSerializer.Serialize(value, _options),
            expiry: expiry,
            when: When.Always,
            flags: CommandFlags.None
        );
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
