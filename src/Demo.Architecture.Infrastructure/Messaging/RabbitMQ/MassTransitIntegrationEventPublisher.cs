using Demo.Architecture.UseCases.Common.Interfaces;
using MassTransit;

namespace Demo.Architecture.Infrastructure.Messaging.RabbitMQ;

public class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
