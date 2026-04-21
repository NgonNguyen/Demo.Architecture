using Demo.Architecture.UseCases.Common.Interfaces;
using MassTransit;

namespace Demo.Architecture.Infrastructure.Messaging.RabbitMQ;

public class MassTransitIntegrationEventPublisher
    (IPublishEndpoint publishEndpoint,
     ISendEndpointProvider sendEndpointProvider)
    : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }

    public async Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken)
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(
            new Uri($"queue:{queueName}")
        );

        await endpoint.Send(message, cancellationToken);
    }
}
