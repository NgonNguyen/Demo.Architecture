using Demo.Architecture.Shared.Serialization;
using System.Text.Json;

namespace Demo.Architecture.WebAPI.Extensions;

public static class JsonExtensions
{
    public static IServiceCollection AddJsonConfiguration(
        this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase;

                options.JsonSerializerOptions.Converters.Add(
                    new UlidJsonConverter());
            });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase;

            options.SerializerOptions.Converters.Add(
                new UlidJsonConverter());
        });

        return services;
    }
}
