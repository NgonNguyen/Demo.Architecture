using Demo.Architecture.WebAPI.Consumer.IntegrationCommands;
using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Consumers.Products;

public class InitializeInventoryConsumer : IConsumer<InitializeInventoryCommand>
{
    public async Task Consume(ConsumeContext<InitializeInventoryCommand> context)
    {
        // Throw an exception to test the retry mechanism of MassTransit.
        // The message will be retried according to the configured retry policy.
        // throw new Exception("Something failed! 123");

        var msg = context.Message;

        Console.WriteLine($"[InitializeInventoryConsumer] {msg.ProductId}");

        await Task.CompletedTask;
    }
}
