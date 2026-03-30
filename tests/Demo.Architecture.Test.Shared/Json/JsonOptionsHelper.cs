using Demo.Architecture.WebAPI.Common.Json;
using System.Text.Json;

namespace Demo.Architecture.Test.Shared.Json;

public static class JsonOptionsHelper
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        options.Converters.Add(new UlidJsonConverter());

        return options;
    }
}
