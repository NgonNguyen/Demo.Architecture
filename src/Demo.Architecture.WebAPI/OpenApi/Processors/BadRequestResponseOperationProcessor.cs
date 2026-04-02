using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class BadRequestResponseOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;
        if (operation == null)
            return true;

        // Apply to POST, PUT, PATCH operations (typically have request bodies)
        var httpMethod = context.OperationDescription.Method?.ToUpperInvariant();
        if (httpMethod != "POST" && httpMethod != "PUT" && httpMethod != "PATCH")
            return true;

        var key = "400";

        if (!context.OperationDescription.Operation.Responses.TryGetValue(key, out var response))
        {
            response = new OpenApiResponse
            {
                Description = "Bad Request"
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
                ["errors"] = new JsonSchemaProperty
                {
                    Type = JsonObjectType.Array,
                    Item = new JsonSchema
                    {
                        Type = JsonObjectType.Object,
                        Properties =
                        {
                            ["field"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                            ["message"] = new JsonSchemaProperty { Type = JsonObjectType.String },
                            ["code"] = new JsonSchemaProperty { Type = JsonObjectType.String }
                        }
                    }
                },
                ["traceId"] = new JsonSchemaProperty { Type = JsonObjectType.String }
            }
        };

        var example = new
        {
            type = "https://httpstatuses.com/400",
            title = "Validation Error",
            status = 400,
            detail = "One or more validation errors occurred.",
            errors = new[]
            {
                new
                {
                    field = "name",
                    message = "Name is required",
                    code = "PRODUCT_NAME_REQUIRED"
                },
                new
                {
                    field = "price",
                    message = "Price must be > 0",
                    code = "PRODUCT_PRICE_INVALID"
                }
            },
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
