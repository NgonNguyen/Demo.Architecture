using Demo.Architecture.WebAPI.Features.Products.Update;

namespace Demo.Architecture.WebAPI.OpenApi.Examples.Products;

public class UpdateProductRequestExampleProvider
{
    public UpdateProductRequest GetExample() => new(
       Name: "Example Product",
       Price: 99.99m
   );
}
