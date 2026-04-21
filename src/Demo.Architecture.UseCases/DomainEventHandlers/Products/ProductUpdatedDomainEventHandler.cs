using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.IntegrationEvents.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Architecture.UseCases.DomainEventHandlers.Products;

public class ProductUpdatedDomainEventHandler(
     ILogger<ProductUpdatedDomainEventHandler> logger,
    IIntegrationEventPublisher publisher)
    : INotificationHandler<ProductUpdatedDomainEvent>
{
    public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ProductUpdatedDomainEvent] Product with ID {ProductId} has been updated. Name: {Name}, Price: {Price}",
            notification.ProductId, notification.Name, notification.Price);

        var integrationEvent = new ProductUpdatedIntegrationEvent
        {
            Id = notification.ProductId,
            Name = notification.Name,
            Price = notification.Price
        };

        await publisher.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation("[ProductUpdatedDomainEvent] Published integration event: {IntegrationEventId}", integrationEvent.Id);
    }
}
