using Demo.Architecture.Infrastructure.Messaging.Extensions;
using Demo.Architecture.Infrastructure.Observability;
using Demo.Architecture.Shared.Serialization;
using Demo.Architecture.UseCases;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.WebAPI.Configurations;
using MassTransit;
using MediatR;

namespace Demo.Architecture.WebAPI.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitServices(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        if (builder.Environment.IsEnvironment("Test"))
        {
            return services;
        }

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ApplyRoutingKeyAttributes(
                    typeof(IntegrationEventsAssemblyMarker).Assembly);

                cfg.UseRawJsonSerializer();

                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.Converters.Add(new UlidJsonConverter());
                    return options;
                });
            });
        });

        services.AddIdempotency();

        return services;
    }
}
