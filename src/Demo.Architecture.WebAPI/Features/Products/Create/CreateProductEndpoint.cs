using Demo.Architecture.UseCases.Features.Products.Commands.Create;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using MediatR;
using NSwag.Annotations;
using NUlid;
using Demo.Architecture.WebAPI.OpenApi.Attributes;
using Demo.Architecture.WebAPI.OpenApi.Examples.Products;

namespace Demo.Architecture.WebAPI.Features.Products.Create;

public class CreateProductEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost("/products", Handle)
            .WithName("CreateProduct")
            .WithTags("Products")
            .Produces<Ulid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    [OpenApiOperation(
        "Create a product",
        "Creates a product."
    )]
    [RequestExample(typeof(CreateProductRequestExampleProvider))]
    internal static async Task<IResult> Handle(
        ISender sender,
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.ToHttpResult();
    }
}
