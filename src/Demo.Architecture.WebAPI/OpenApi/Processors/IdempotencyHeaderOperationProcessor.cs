using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NUlid;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class IdempotencyHeaderOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var method = context.OperationDescription.Method;

        if (method == OpenApiOperationMethod.Post ||
            method == OpenApiOperationMethod.Put ||
            method == OpenApiOperationMethod.Delete)
        {
            var parameters = context.OperationDescription.Operation.Parameters;

            // Avoid duplicate header
            if (!parameters.Any(p => p.Name == "Idempotency-Key"))
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = "Idempotency-Key",
                    Kind = OpenApiParameterKind.Header,
                    IsRequired = true,
                    Description = "Unique request key to ensure idempotent operations",
                    Schema = new NJsonSchema.JsonSchema
                    {
                        Type = NJsonSchema.JsonObjectType.String,
                        Example = Ulid.NewUlid()
                    }
                });
            }
        }

        return true;
    }
}
