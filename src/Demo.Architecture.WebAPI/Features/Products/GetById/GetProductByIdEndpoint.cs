using Demo.Architecture.UseCases.Features.Products.Queries.GetById;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using Demo.Architecture.WebAPI.OpenApi.Attributes;
using Demo.Architecture.WebAPI.OpenApi.Examples.Products;
using MediatR;
using NSwag.Annotations;

namespace Demo.Architecture.WebAPI.Features.Products.GetById;

public class GetProductByIdEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/products/{id}", Handle)
            .WithName("GetProductById")
            .WithTags("Products")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    [OpenApiOperation(
        "Get product by Id",
        "Retrieves a product filter by Id."
    )]
    [ResponseExample(typeof(GetProductByIdResponseExampleProvider), 200)]
    internal static async Task<IResult> Handle(
        ISender sender,
        [AsParameters] GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToHttpResult();
    }
}
