using System.Reflection;

namespace Demo.Architecture.WebAPI.Common.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointBuilders = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEndpointBuilder).IsAssignableFrom(t)
                        && !t.IsInterface
                        && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IEndpointBuilder>();

        foreach (var endpoint in endpointBuilders)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
