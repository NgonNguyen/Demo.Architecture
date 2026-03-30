namespace Demo.Architecture.WebAPI.Common.Endpoints;

public interface IEndpointBuilder
{
    void MapEndpoint(IEndpointRouteBuilder routeBuilder);
}