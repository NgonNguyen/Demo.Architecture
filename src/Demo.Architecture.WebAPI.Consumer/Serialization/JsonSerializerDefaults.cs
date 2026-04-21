using System.Text.Json;

namespace Demo.Architecture.WebAPI.Consumer.Serialization;

public static class JsonSerializerDefaults
{
    public static JsonSerializerOptions Options => new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new UlidJsonConverter()
        }
    };
}
