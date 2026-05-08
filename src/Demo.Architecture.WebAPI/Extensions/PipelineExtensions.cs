using Demo.Architecture.Infrastructure.Observability;
using Demo.Architecture.UseCases.Common.Behaviors;
using MediatR;

namespace Demo.Architecture.WebAPI.Extensions;

public static class PipelineExtensions
{
    public static IServiceCollection AddPipelineBehaviors(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        if (builder.Environment.IsEnvironment("Test"))
        {
            return services;
        }

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(TracingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(MetricsBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(CachingBehavior<,>));

        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(CacheInvalidationBehavior<,>));

        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(InjectUserInfoBehavior<,>));

        return services;
    }
}
