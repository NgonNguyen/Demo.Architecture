using Demo.Architecture.UseCases.Common.Messaging.Attributes;
using NUlid;
using System.Text.Json.Serialization;

namespace Demo.Architecture.UseCases.IntegrationEvents.Products;

[RoutingKey("product.updated")]
public class ProductUpdatedIntegrationEvent
{
    [JsonPropertyName("id")]
    public Ulid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
