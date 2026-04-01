using Demo.Architecture.UseCases.Features.Products.Commands.Create;

namespace Demo.Architecture.WebAPI.OpenApi.Examples.Products;

public class CreateProductRequestExampleProvider
{
    public CreateProductCommand GetExample() => new(
        Name: "Example Product",
        Price: 99.99m
    );
}
