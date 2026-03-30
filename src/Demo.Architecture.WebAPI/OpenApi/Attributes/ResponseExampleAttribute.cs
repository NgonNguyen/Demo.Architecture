namespace Demo.Architecture.WebAPI.OpenApi.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ResponseExampleAttribute : Attribute
{
    public Type ExampleProviderType { get; }

    public int StatusCode { get; }

    public ResponseExampleAttribute(Type exampleProviderType, int statusCode = 200)
    {
        ExampleProviderType = exampleProviderType;
        StatusCode = statusCode;
    }
}
