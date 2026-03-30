using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Infrastructure.Data;

namespace Demo.Architecture.Test.Shared.Seeders;

public static class ProductSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var products = new List<Product>
        {
            Product.Create("Product A", 1000).Value,
            Product.Create("Product B", 900).Value,
            Product.Create("Product C", 500).Value
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
