using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Common.Messaging.Routing;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using AppExchangeType = RabbitMQ.Client.ExchangeType;

namespace Demo.Architecture.Infrastructure.Messaging.RabbitMQ;

public class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IConnection _connection;

    public RabbitMqIntegrationEventPublisher(IConnection connection)
    {
        _connection = connection;
    }

    public Task PublishAsync<T>(
        T integrationEvent,
        IntegrationEventOptions options,
        CancellationToken cancellationToken = default)
    {
        /*using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: options.ExchangeName,
            type: options.ExchangeType,
            durable: true);

        var message = JsonSerializer.Serialize(
            integrationEvent,
            Shared.Serialization.JsonSerializerDefaults.Options);

        var body = Encoding.UTF8.GetBytes(message);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        // Fanout ignores routingKey → use empty string
        var finalRoutingKey = options.ExchangeType == AppExchangeType.Fanout
            ? string.Empty
            : options.RoutingKey ?? RoutingKeyResolver.GetRoutingKey<T>();

        channel.BasicPublish(
            exchange: options.ExchangeName,
            routingKey: finalRoutingKey,
            basicProperties: properties,
            body: body
        );*/

        return Task.CompletedTask;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
    {
        // Direct
        /*var options = new IntegrationEventOptions
        {
            ExchangeName = "integration-events",
            ExchangeType = AppExchangeType.Direct,
            RoutingKey = RoutingKeyResolver.GetRoutingKey<T>()
        };*/

        // Topic
        /*var options = new IntegrationEventOptions
        {
            ExchangeName = "integration-events-topic",
            ExchangeType = AppExchangeType.Topic,
            RoutingKey = RoutingKeyResolver.GetRoutingKey<T>()
        };*/

        // Fanout
        var options = new IntegrationEventOptions
        {
            ExchangeName = "integration-events-fanout",
            ExchangeType = AppExchangeType.Fanout
        };

        PublishAsync(integrationEvent, options);

        return Task.CompletedTask;
    }

    public Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class IntegrationEventOptions
{
    public string ExchangeName { get; set; } = "integration-events";
    public string ExchangeType { get; set; }
    public string? RoutingKey { get; set; }

    public IntegrationEventOptions()
    {
        ExchangeType = ExchangeType ?? AppExchangeType.Direct;
    }
}