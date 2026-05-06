using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.IntegrationEvents.Products;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Demo.Architecture.UseCases.DomainEventHandlers.Products;

public class ProductUpdatedDomainEventHandler(
     ILogger<ProductUpdatedDomainEventHandler> logger,
    IIntegrationEventPublisher publisher)
    : INotificationHandler<ProductUpdatedDomainEvent>
{
    public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var traceId = Guid.Parse(Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString());

        logger.LogInformation(
            "[TraceId: {TraceId}] Product updated: {ProductId} - {ProductName} - {Price}",
            traceId,
            notification.ProductId,
            notification.Name,
            notification.Price);

        var integrationEvent = new ProductUpdatedIntegrationEvent
        {
            Id = notification.ProductId,
            Name = notification.Name,
            Price = notification.Price
        };

        await publisher.PublishAsync(integrationEvent, traceId, cancellationToken);

        logger.LogInformation("[ProductUpdatedDomainEvent] Published integration event: {IntegrationEventId}", integrationEvent.Id);
    }
}
