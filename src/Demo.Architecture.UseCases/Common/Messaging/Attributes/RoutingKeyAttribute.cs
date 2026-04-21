namespace Demo.Architecture.UseCases.Common.Messaging.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RoutingKeyAttribute : Attribute
{
    public string Key { get; }

    public RoutingKeyAttribute(string key)
    {
        Key = key;
    }
}
