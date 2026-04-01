using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class NotFoundResponseOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;
        if (operation == null)
            return true;

        if (!operation.Parameters.Any(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase)))
            return true;

        var key = "404";

        if (!context.OperationDescription.Operation.Responses.TryGetValue(key, out var response))
        {
            response = new OpenApiResponse
            {
                Description = "Not Found"
            };

            context.OperationDescription.Operation.Responses[key] = response;
        }

        var schema = new JsonSchema
        {
            Type = JsonObjectType.Object,
            Properties =
            {
                ["type"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                ["title"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                ["status"] = new JsonSchemaProperty { Type = JsonObjectType.Integer },
                ["detail"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                ["instance"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                ["errorCode"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                ["traceId"] = new JsonSchemaProperty { Type = JsonObjectType.String },
            }
        };

        var example = new
        {
            type = "https://httpstatuses.com/404",
            title = "Not Found",
            status = 404,
            detail = "Not found",
            instance = "/api/example/{id}",
            errorCode = "NOT_FOUND",
            traceId = "00-abcdef1234567890"
        };

        var content = response.Content;

        if (content.ContainsKey("application/json"))
        {
            content["application/json"].Schema = schema;
            content["application/json"].Example = example;
        }
        else
        {
            content.Add("application/json", new OpenApiMediaType
            {
                Schema = schema,
                Example = example
            });
        }

        return true;
    }
}
