using Demo.Architecture.WebAPI.Consumer.IntegrationEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using AppExchangeType = RabbitMQ.Client.ExchangeType;

namespace Demo.Architecture.WebAPI.Consumer.Service;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly IModel _channel;
    private IntegrationEventConsumerOptions options;

    public RabbitMqConsumerService(
        IConnection connection)
    {
        // Direct
        /*options = new IntegrationEventConsumerOptions
        {
            ExchangeName = "integration-events",
            ExchangeType = AppExchangeType.Direct,
            QueueName = "product-created-queue",
            RoutingKeys = { "product.created" }
        };*/

        // Topic
        // RoutingKeys: 
        // - "product.created" → match only "product.created"
        // - "product." → match all events starting with "product." (e.g., "product.created", "product.updated")
        // - "*.created" → match all events ending with ".created" (e.g., "product.created", "order.created")
        // - "#" → match all events (not recommended for production)
        /*options = new IntegrationEventConsumerOptions
        {
            ExchangeName = "integration-events-topic",
            ExchangeType = AppExchangeType.Topic,
            QueueName = "product-created-queue",
            RoutingKeys = { "product.*" }
        };*/

        // Fanout
        options = new IntegrationEventConsumerOptions
        {
            ExchangeName = "integration-events-fanout",
            ExchangeType = AppExchangeType.Fanout,
            QueueName = "product-created-queue"
        };

        _channel = connection.CreateModel();

        // 1. Declare exchange
        _channel.ExchangeDeclare(
            exchange: options.ExchangeName,
            type: options.ExchangeType,
            durable: true);

        // 2. Declare queue
        _channel.QueueDeclare(
            queue: options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // 3. Bind queue based on exchange type
        BindQueue(options);
    }

    private void BindQueue(IntegrationEventConsumerOptions option)
    {
        // Fanout → ignore routing keys
        if (option.ExchangeType == AppExchangeType.Fanout)
        {
            _channel.QueueBind(
                queue: option.QueueName,
                exchange: option.ExchangeName,
                routingKey: string.Empty);

            return;
        }

        // Direct / Topic → use routing keys
        foreach (var routingKey in option.RoutingKeys)
        {
            _channel.QueueBind(
                queue: option.QueueName,
                exchange: option.ExchangeName,
                routingKey: routingKey);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                var message = JsonSerializer.Deserialize<ProductCreatedIntegrationEvent>(
                    json,
                    Serialization.JsonSerializerDefaults.Options);

                if (message != null)
                {
                    await HandleMessage(message);
                }

                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                _channel.BasicNack(args.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: options.QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private Task HandleMessage(ProductCreatedIntegrationEvent message)
    {
        // 👉 Your business logic here
        Console.WriteLine($"[Consumer] Received product: {message.Id.ToString()} - {message.Name} - {message.Price}");

        return Task.CompletedTask;
    }
}

public class IntegrationEventConsumerOptions
{
    public string ExchangeName { get; set; } = "integration-events";

    public string ExchangeType { get; set; } = AppExchangeType.Direct;

    public string QueueName { get; set; } = default!;

    // Support multiple bindings (important for Topic)
    public List<string> RoutingKeys { get; set; } = new();
}