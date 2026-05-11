using Demo.Architecture.UseCases.Common.Models;
using Demo.Architecture.UseCases.Features.Coffees.Queries.GetList;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Extensions;
using MediatR;
using NSwag.Annotations;

namespace Demo.Architecture.WebAPI.Features.Coffees.GetList;

public class GetListCoffeesEndpoint : IEndpointBuilder
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/coffees", Handle)
            .WithName("GetListCoffees")
            .WithTags("Coffees")
            .Produces<PagedResult<GetListCoffeesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            // .RequireAuthorization("ProductScope", "AdminOnly")
            // .RequireAuthorization("ProductScope", "ProductRead")
            ;
    }

    [OpenApiOperation(
        "Get coffees",
        "Retrieves a paginated list of coffees."
    )]
    // [ResponseExample(typeof(GetListCoffeesResponseExampleProvider), 200)]
    internal static async Task<IResult> Handle(
        ISender sender,
        [AsParameters] GetListCoffeesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return result.ToHttpResult();
    }
}
