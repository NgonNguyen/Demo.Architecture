using Demo.Architecture.Infrastructure.Idempotency;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Idempotency;
using MediatR;

namespace Demo.Architecture.WebAPI.Configurations;

public static class IdempotencyConfiguration
{
    public static IServiceCollection AddIdempotency(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.Configure<IdempotencyOptions>(options =>
        {
            options.CacheTtl = TimeSpan.FromMinutes(10);
            options.LockTtl = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

        return services;
    }
}
