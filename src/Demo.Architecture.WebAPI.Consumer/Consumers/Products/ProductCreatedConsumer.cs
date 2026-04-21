using Demo.Architecture.WebAPI.Consumer.IntegrationEvents;
using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Consumers.Products;

public class ProductCreatedConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    public Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var msg = context.Message;

        Console.WriteLine($"[ProductCreatedConsumer] {msg.Id} - {msg.Name} - {msg.Price}");

        return Task.CompletedTask;
    }
}