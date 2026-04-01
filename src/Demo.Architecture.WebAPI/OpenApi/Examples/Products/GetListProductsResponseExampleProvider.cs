using NUlid;
using Demo.Architecture.UseCases.Common.Models;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;

namespace Demo.Architecture.WebAPI.OpenApi.Examples.Products;

public class GetListProductsResponseExampleProvider
{
    public PagedResult<GetListProductsResponse> GetExample()
    {
        var items = new List<GetListProductsResponse>
        {
            new GetListProductsResponse(Ulid.NewUlid(), "Example Product 1", 999.99m),
            new GetListProductsResponse(Ulid.NewUlid(), "Example Product 2", 56.00m),
            new GetListProductsResponse(Ulid.NewUlid(), "Example Product 3", 22.34m),
            new GetListProductsResponse(Ulid.NewUlid(), "Example Product 4", 216.78m),
            new GetListProductsResponse(Ulid.NewUlid(), "Example Product 5", 34.2m)
        };

        return new PagedResult<GetListProductsResponse>(items, totalCount: 100, page: 1, pageSize: 5);
    }
}
