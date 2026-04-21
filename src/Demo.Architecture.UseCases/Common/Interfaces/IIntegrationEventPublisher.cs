namespace Demo.Architecture.UseCases.Common.Interfaces;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default);
    Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken);
}
