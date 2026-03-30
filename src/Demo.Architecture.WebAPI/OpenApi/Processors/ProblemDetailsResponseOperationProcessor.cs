using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class ProblemDetailsResponseOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (!context.OperationDescription.Operation.Responses.TryGetValue("500", out var response))
        {
            response = new OpenApiResponse
            {
                Description = "Internal Server Error"
            };

            context.OperationDescription.Operation.Responses["500"] = response;
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
                ["traceId"] = new JsonSchemaProperty { Type = JsonObjectType.String }
            }
        };

        var example = new
        {
            type = "https://httpstatuses.com/500",
            title = "Internal Server Error",
            status = 500,
            detail = "An unexpected error occurred.",
            instance = "/api/example",
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
