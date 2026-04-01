using Demo.Architecture.WebAPI.OpenApi.Attributes;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class RequestExampleOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var methodInfo = context.MethodInfo;
        if (methodInfo == null) return true;

        var attributes = methodInfo
            .GetCustomAttributes(typeof(RequestExampleAttribute), false)
            .Cast<RequestExampleAttribute>();

        foreach (var attr in attributes)
        {
            var provider = Activator.CreateInstance(attr.ExampleProviderType);
            var method = attr.ExampleProviderType.GetMethod("GetExample");
            if (method == null) continue;

            var exampleObj = method.Invoke(provider, null);

            // find request body parameter
            var requestBody = context.OperationDescription.Operation.RequestBody;
            if (requestBody == null) continue;

            if (!requestBody.Content.ContainsKey("application/json")) continue;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                options.Converters.Add(new Demo.Architecture.WebAPI.Common.Json.UlidJsonConverter());

                var json = JsonSerializer.Serialize(exampleObj, options);
                var jtoken = JToken.Parse(json);
                requestBody.Content["application/json"].Example = jtoken;
            }
            catch
            {
                requestBody.Content["application/json"].Example = exampleObj;
            }
        }

        return true;
    }
}
