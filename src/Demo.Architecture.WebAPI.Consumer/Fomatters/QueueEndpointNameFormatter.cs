using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Fomatters;

public class QueueEndpointNameFormatter : KebabCaseEndpointNameFormatter
{
    public QueueEndpointNameFormatter(string? prefix = null)
        : base(prefix, includeNamespace: false)
    {
    }

    public override string Consumer<T>()
    {
        var name = base.Consumer<T>();
        // e.g. product-created-consumer

        if (name.EndsWith("-consumer"))
            name = name[..^("-consumer".Length)];

        return $"{name}-queue";
    }
}