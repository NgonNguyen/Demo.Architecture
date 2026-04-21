using MassTransit;
using System.Reflection;

namespace Demo.Architecture.WebAPI.Consumer.Extensions;

public static class MassTransitExtensions
{
    public static void ApplyEntityNamesFromAssembly(
        this IRabbitMqBusFactoryConfigurator cfg,
        Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<EntityNameAttribute>() != null);

        var method = typeof(IBusFactoryConfigurator)
            .GetMethods()
            .First(m => m.Name == "Message" && m.IsGenericMethod);

        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<EntityNameAttribute>()!;

            var generic = method.MakeGenericMethod(type);

            generic.Invoke(cfg, new object[]
            {
                new Action<dynamic>(x => x.SetEntityName(attr.EntityName))
            });
        }
    }
}
