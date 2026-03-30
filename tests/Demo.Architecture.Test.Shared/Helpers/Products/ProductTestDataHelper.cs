using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Features.Products.Queries.GetAll;
using NUlid;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.Test.Shared.Helpers.Products;

public static class ProductTestDataHelper
{
    public static AppModels.PagedResult<GetAllProductsResponse> CreatePagedResponse()
    {
        return new AppModels.PagedResult<GetAllProductsResponse>(
            new List<GetAllProductsResponse>
            {
                new(
                    Ulid.NewUlid(),
                    TestConstants.ValidProductNameA,
                    TestConstants.ValidPriceA)
            },
            1,
            1,
            20
        );
    }
}
