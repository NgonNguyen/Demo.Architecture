using Demo.Architecture.WebAPI.OpenApi.Attributes;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

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

            var example = method.Invoke(provider, null);

            var responses = context.OperationDescription.Operation.Responses;

            var statusCode = attr.StatusCode.ToString();

            if (!responses.ContainsKey(statusCode))
                continue;

            var response = responses[statusCode];

            if (response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"].Example = example;
            }
        }

        return true;
    }
}
