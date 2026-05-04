using Demo.Architecture.UseCases.Features.Products.Commands.Delete;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using MediatR;
using NSwag.Annotations;
using NUlid;

namespace Demo.Architecture.WebAPI.Features.Products.Delete;

public class DeleteProductEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapDelete("/products/{id}", Handle)
            .WithName("DeleteProduct")
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("ProductScope", "ProductWrite");
    }

    [OpenApiOperation(
        "Delete a product",
        "Deletes a product."
    )]
    internal static async Task<IResult> Handle(
        ISender sender,
        Ulid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await sender.Send(command, cancellationToken);
        return result.ToHttpResult();
    }
}
