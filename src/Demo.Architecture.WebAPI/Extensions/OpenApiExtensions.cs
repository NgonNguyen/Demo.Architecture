using Demo.Architecture.WebAPI.OpenApi.Processors;

namespace Demo.Architecture.WebAPI.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApiDocument(config =>
        {
            config.Title = "Demo Architecture API";

            config.UseControllerSummaryAsTagDescription = true;

            config.OperationProcessors.Add(
                new InternalServerErrorResponseOperationProcessor());

            config.OperationProcessors.Add(
                new ResponseExampleOperationProcessor());

            config.OperationProcessors.Add(
                new RequestExampleOperationProcessor());

            config.OperationProcessors.Add(
                new NotFoundResponseOperationProcessor());

            config.OperationProcessors.Add(
                new BadRequestResponseOperationProcessor());

            config.OperationProcessors.Add(
                new QueryExampleOperationProcessor());

            config.OperationProcessors.Add(
                new IdempotencyHeaderOperationProcessor());
        });

        return services;
    }
}