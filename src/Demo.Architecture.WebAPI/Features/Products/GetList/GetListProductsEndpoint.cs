using Demo.Architecture.UseCases.Common.Models;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using Demo.Architecture.WebAPI.OpenApi.Attributes;
using Demo.Architecture.WebAPI.OpenApi.Examples.Products;
using MediatR;
using NSwag.Annotations;

namespace Demo.Architecture.WebAPI.Features.Products.GetList;

public class GetAllProductsEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/products", Handle)
            .WithName("GetListProducts")
            .WithTags("Products")
            .Produces<PagedResult<GetListProductsResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            // .RequireAuthorization("ProductScope", "AdminOnly")
            .RequireAuthorization("ProductScope", "ProductRead")
            ;
    }

    [OpenApiOperation(
        "Get products",
        "Retrieves a paginated list of products."
    )]
    [ResponseExample(typeof(GetListProductsResponseExampleProvider), 200)]
    internal static async Task<IResult> Handle(
        ISender sender,
        [AsParameters] GetListProductsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToHttpResult();
    }
}
