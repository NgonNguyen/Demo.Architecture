using Demo.Architecture.Core.Entities.Products;
using NUlid;
using System.Text.Json.Serialization;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetById;

public sealed record GetProductByIdResponse(
    [property: JsonPropertyName("id")] Ulid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price
)
{
    public static GetProductByIdResponse FromEntity(Product product)
        => new(product.Id.Value, product.Name, product.Price.Value);
}
