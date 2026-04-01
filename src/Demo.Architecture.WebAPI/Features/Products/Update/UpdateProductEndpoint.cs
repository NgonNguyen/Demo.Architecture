using Demo.Architecture.UseCases.Features.Products.Commands.Update;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using Demo.Architecture.WebAPI.OpenApi.Attributes;
using Demo.Architecture.WebAPI.OpenApi.Examples.Products;
using MediatR;
using NSwag.Annotations;
using NUlid;
using System.ComponentModel;

namespace Demo.Architecture.WebAPI.Features.Products.Update;

public class UpdateProductEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPut("/products/{id}", Handle)
            .WithName("UpdateProduct")
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    [OpenApiOperation(
        "Update a product",
        "Updates a product."
    )]
    [RequestExample(typeof(UpdateProductRequestExampleProvider))]
    internal static async Task<IResult> Handle(
        ISender sender,
        Ulid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Price);

        var result = await sender.Send(command, cancellationToken);
        return result.ToHttpResult();
    }
}

public record UpdateProductRequest(
    [property: Description("Name of product.")] string Name,
    [property: Description("Price of product.")] decimal Price
);