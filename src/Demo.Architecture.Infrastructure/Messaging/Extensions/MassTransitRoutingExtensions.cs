using Demo.Architecture.UseCases.Common.Messaging.Attributes;
using MassTransit;

namespace Demo.Architecture.Infrastructure.Messaging.Extensions;

public static class MassTransitRoutingExtensions
{
    public static void ApplyRoutingKeyAttributes(
        this IRabbitMqBusFactoryConfigurator cfg,
        Assembly assembly)
    {
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<RoutingKeyAttribute>() != null);

        var messageMethod = typeof(IBusFactoryConfigurator) // ✅ FIX HERE
            .GetMethods()
            .First(m => m.Name == "Message" && m.IsGenericMethod);

        foreach (var type in messageTypes)
        {
            var attr = type.GetCustomAttribute<RoutingKeyAttribute>()!;

            var genericMethod = messageMethod.MakeGenericMethod(type);

            genericMethod.Invoke(cfg, new object[]
            {
                new Action<dynamic>(x => x.SetEntityName(attr.Key))
            });
        }
    }
}