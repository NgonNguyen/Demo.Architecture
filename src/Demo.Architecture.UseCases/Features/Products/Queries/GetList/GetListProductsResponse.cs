using NUlid;
using System.Text.Json.Serialization;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetList;

public record GetListProductsResponse(
    [property: JsonPropertyName("id")] Ulid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price
);
