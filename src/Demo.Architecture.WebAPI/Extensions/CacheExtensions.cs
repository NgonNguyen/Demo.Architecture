using Demo.Architecture.Infrastructure.Caching;
using Demo.Architecture.UseCases.Common.Interfaces;
using StackExchange.Redis;

namespace Demo.Architecture.WebAPI.Extensions;

public static class CacheExtensions
{
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString =
                configuration.GetConnectionString("Redis");

            return ConnectionMultiplexer.Connect(connectionString!);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}