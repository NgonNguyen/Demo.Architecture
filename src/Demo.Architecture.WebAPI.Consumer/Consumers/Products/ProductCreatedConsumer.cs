using Demo.Architecture.WebAPI.Consumer.IntegrationEvents;
using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Consumers.Products;

public class ProductCreatedConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    public Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var traceId = context.CorrelationId?.ToString() ?? context.Headers.Get<string>("trace-id");
        var msg = context.Message;

        Console.WriteLine($"[ProductCreatedConsumer] [TraceId: {traceId}] {msg.Id} - {msg.Name} - {msg.Price}");

        return Task.CompletedTask;
    }
}