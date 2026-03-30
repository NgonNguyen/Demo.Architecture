using NUlid;

namespace Demo.Architecture.WebAPI.OpenApi.Examples.Products;

public class GetAllProductsExampleProvider
{
    public object GetExample() => new
    {
        page = 1,
        pageSize = 5,
        totalCount = 100,
        items = new[]
        {
            new { id = Ulid.NewUlid(), name = "Example Product 1", price = 999.99 },
            new { id = Ulid.NewUlid(), name = "Example Product 2", price = 56.00 },
            new { id = Ulid.NewUlid(), name = "Example Product 3", price = 22.34 },
            new { id = Ulid.NewUlid(), name = "Example Product 4", price = 216.78 },
            new { id = Ulid.NewUlid(), name = "Example Product 5", price = 34.2 }
        }

    };
}
