using Demo.Architecture.Infrastructure.Features.Products;
using Demo.Architecture.Infrastructure.Identity;
using Demo.Architecture.Infrastructure.Messaging.RabbitMQ;
using Demo.Architecture.UseCases.Common.Identity;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Rules;

namespace Demo.Architecture.WebAPI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IIntegrationEventPublisher,
            MassTransitIntegrationEventPublisher>();

        services.AddScoped<IProductUniquenessChecker,
            ProductUniquenessChecker>();

        services.AddControllers();

        return services;
    }
}