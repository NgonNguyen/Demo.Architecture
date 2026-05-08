using Demo.Architecture.UseCases;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Features.Products.Rules;
using FluentValidation;

namespace Demo.Architecture.WebAPI.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRServices(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(ApplicationAssemblyMarker).Assembly);

            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(
            typeof(IProductUniquenessChecker).Assembly);

        return services;
    }
}
