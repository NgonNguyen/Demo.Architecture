using Demo.Architecture.Infrastructure.ExternalServices;
using Demo.Architecture.UseCases.ExternalServices;
using Polly;

namespace Demo.Architecture.WebAPI.Extensions;

public static class ExternalApisExtensions
{
    public static IServiceCollection AddExternalApis(
        this IServiceCollection services)
    {
        services
            .AddHttpClient<ICoffeeApiClient, CoffeeApiClient>(
                client =>
                {
                    client.BaseAddress =
                        new Uri("https://api.sampleapis.com");
                })
            .AddStandardResilienceHandler(options =>
            {
                // Timeout
                options.AttemptTimeout.Timeout =
                    TimeSpan.FromSeconds(3);

                options.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(10);

                // Retry
                options.Retry.MaxRetryAttempts = 3;

                options.Retry.Delay =
                    TimeSpan.FromSeconds(1);

                options.Retry.BackoffType =
                    DelayBackoffType.Exponential;

                // Circuit breaker
                options.CircuitBreaker.FailureRatio = 0.5;

                options.CircuitBreaker.MinimumThroughput = 4;

                options.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(10);

                options.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(30);

                // Bulkhead / isolation
                options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 5;

                options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 10;

                // Retry logging
                options.Retry.OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Retry #{args.AttemptNumber}");

                    return default;
                };
            });

        return services;
    }
}
