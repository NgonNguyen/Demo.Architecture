using Demo.Architecture.WebAPI.Consumer.IntegrationCommands;
using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Consumers.Products;

public class InitializeInventoryConsumer : IConsumer<InitializeInventoryCommand>
{
    public async Task Consume(ConsumeContext<InitializeInventoryCommand> context)
    {
        var msg = context.Message;

        Console.WriteLine($"[InitializeInventoryConsumer] {msg.ProductId}");

        await Task.CompletedTask;
    }
}
