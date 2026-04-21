using MassTransit;
using NUlid;
using System.Text.Json.Serialization;

namespace Demo.Architecture.WebAPI.Consumer.IntegrationEvents;

[EntityName("product.created")]
public class ProductCreatedIntegrationEvent
{
    [JsonPropertyName("id")]
    public Ulid Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
