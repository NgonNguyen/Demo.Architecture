using Demo.Architecture.UseCases.Common.Models;
using Demo.Architecture.UseCases.Features.Products.Queries.GetAll;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using Demo.Architecture.WebAPI.OpenApi.Attributes;
using Demo.Architecture.WebAPI.OpenApi.Examples.Products;
using MediatR;
using NSwag.Annotations;

namespace Demo.Architecture.WebAPI.Features.Products.GetAll;

public class GetAllProductsEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/products", Handle)
            .WithName("GetAllProducts")
            .WithTags("Products")
            .Produces<PagedResult<GetAllProductsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    [OpenApiOperation(
        "Get products",
        "Retrieves a paginated list of products."
    )]
    [ResponseExample(typeof(GetAllProductsExampleProvider), 200)]
    internal static async Task<IResult> Handle(
        ISender sender,
        [AsParameters] GetAllProductsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToHttpResult();
    }
}
