using Demo.Architecture.UseCases.ExternalServices;
using MediatR;
using Microsoft.Extensions.Logging;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Coffees.Queries.GetList;

public class GetListCoffeesHandler(
    ILogger<GetListCoffeesHandler> logger,
    ICoffeeApiClient coffeeApiClient)
    : IRequestHandler<GetListCoffeesQuery, Result<AppModels.PagedResult<GetListCoffeesResponse>>>
{
    public async Task<Result<AppModels.PagedResult<GetListCoffeesResponse>>> Handle(
        GetListCoffeesQuery request, CancellationToken cancellationToken)
    {
        var query  = await coffeeApiClient.GetHotCoffeeAsync(cancellationToken);

        var total = query.Count();

        var data = query.Skip(request.Skip).Take(request.PageSize)
            .Select(x => new GetListCoffeesResponse(
                x.Id,
                x.Title,
                x.Description,
                x.Image 
            ))
            .ToList();

        return Result.Success(new AppModels.PagedResult<GetListCoffeesResponse>(
            data,
            total,
            request.Page,
            request.PageSize));
    }
}
