using Demo.Architecture.UseCases.Common.Messaging.Attributes;

namespace Demo.Architecture.UseCases.Common.Messaging.Routing;

public static class RoutingKeyResolver
{
    public static string GetRoutingKey<T>()
    {
        var attr = typeof(T).GetCustomAttributes(typeof(RoutingKeyAttribute), false)
            .FirstOrDefault() as RoutingKeyAttribute;

        if (attr == null)
            throw new Exception($"RoutingKey not defined for {typeof(T).Name}");

        return attr.Key;
    }
}
