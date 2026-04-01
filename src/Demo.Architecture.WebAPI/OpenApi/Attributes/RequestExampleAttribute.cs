namespace Demo.Architecture.WebAPI.OpenApi.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RequestExampleAttribute : Attribute
{
    public Type ExampleProviderType { get; }

    public RequestExampleAttribute(Type exampleProviderType)
    {
        ExampleProviderType = exampleProviderType;
    }
}
