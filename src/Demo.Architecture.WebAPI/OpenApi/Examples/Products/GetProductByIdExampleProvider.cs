using NUlid;
using Demo.Architecture.UseCases.Features.Products.Queries.GetById;

namespace Demo.Architecture.WebAPI.OpenApi.Examples.Products;

public class GetProductByIdExampleProvider
{
    public GetProductByIdResponse GetExample() =>
        new(
            Id: Ulid.NewUlid(),
            Name: "Example Product",
            Price: 99.99m
        );
}