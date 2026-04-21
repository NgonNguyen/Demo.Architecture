using Demo.Architecture.WebAPI.Consumer.IntegrationEvents;
using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Consumers.Products;

public class ProductUpdatedConsumer : IConsumer<ProductUpdatedIntegrationEvent>
{
    public Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
    {
        var msg = context.Message;

        Console.WriteLine($"[ProductUpdatedConsumer] {msg.Id} - {msg.Name} - {msg.Price}");
        return Task.CompletedTask;
    }
}