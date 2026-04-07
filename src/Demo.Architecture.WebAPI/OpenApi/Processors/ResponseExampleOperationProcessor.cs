using Demo.Architecture.Shared.Serialization;
using Demo.Architecture.WebAPI.OpenApi.Attributes;
using Newtonsoft.Json.Linq;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Text.Json;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class ResponseExampleOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var methodInfo = context.MethodInfo;

        if (methodInfo == null)
            return true;

        var attributes = methodInfo
            .GetCustomAttributes(typeof(ResponseExampleAttribute), false)
            .Cast<ResponseExampleAttribute>();

        foreach (var attr in attributes)
        {
            var provider = Activator.CreateInstance(attr.ExampleProviderType);

            var method = attr.ExampleProviderType.GetMethod("GetExample");

            if (method == null)
                continue;

            var exampleObj = method.Invoke(provider, null);

            var responses = context.OperationDescription.Operation.Responses;

            var statusCode = attr.StatusCode.ToString();

            if (!responses.ContainsKey(statusCode))
                continue;

            var response = responses[statusCode];

            if (!response.Content.ContainsKey("application/json"))
                continue;

            // Serialize the example to JSON using System.Text.Json with camelCase naming
            // and the Ulid converter, then assign the resulting JsonElement as the example
            // so the OpenAPI document shows camelCase property names.
            try
            {
                var json = JsonSerializer.Serialize(exampleObj, Shared.Serialization.JsonSerializerDefaults.Options);

                // Parse to a Newtonsoft JToken so NSwag/Swagger UI will embed the raw JSON
                // instead of serializing a JsonElement object (which yields { "ValueKind": 1 }).
                var jtoken = JToken.Parse(json);
                response.Content["application/json"].Example = jtoken;
            }
            catch
            {
                // Fallback: assign the raw object if serialization fails
                response.Content["application/json"].Example = exampleObj;
            }
        }

            // If no explicit response example provided and the operation defines a 201 response
            // with a string schema (commonly used for created-id responses like ULID),
            // set a default example (ULID string) so Swagger shows a useful value.
            try
            {
                var responses = context.OperationDescription.Operation.Responses;
                var createdStatus = "201";
                if (responses.ContainsKey(createdStatus))
                {
                    var createdResponse = responses[createdStatus];

                    if (createdResponse.Content.ContainsKey("application/json"))
                    {
                        var schema = createdResponse.Content["application/json"].Schema;
                        if (schema != null)
                        {
                            var ulid = NUlid.Ulid.NewUlid().ToString();
                            createdResponse.Content["application/json"].Example = JToken.FromObject(ulid);
                    }
                    }
                }
            }
            catch
            {
                // ignore
            }

        return true;
    }
}
