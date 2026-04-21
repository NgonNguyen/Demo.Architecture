using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.IntegrationEvents.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Architecture.UseCases.DomainEventHandlers.Products;

public class ProductCreatedDomainEventHandler(
    ILogger<ProductCreatedDomainEventHandler> logger,
    IIntegrationEventPublisher publisher)
    : INotificationHandler<ProductCreatedDomainEvent>
{
    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ProductCreatedDomainEvent]  Product created: {ProductId} - {ProductName}", notification.Product.Id, notification.Product.Name);

        var integrationEvent = new ProductCreatedIntegrationEvent
        {
            Id = notification.Product.Id.Value,
            Name = notification.Product.Name,
            Price = notification.Product.Price.Value
        };

        await publisher.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation("[ProductCreatedDomainEvent] Published integration event: {IntegrationEventId}", integrationEvent.Id);
    }
}
