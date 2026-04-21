using MassTransit;
using NUlid;
using System.Text.Json.Serialization;

namespace Demo.Architecture.WebAPI.Consumer.IntegrationCommands;

[EntityName("initialize-inventory")]
public class InitializeInventoryCommand
{
    [JsonPropertyName("productId")]
    public Ulid ProductId { get; set; }
}
