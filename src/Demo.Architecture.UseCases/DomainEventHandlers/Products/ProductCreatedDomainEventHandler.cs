using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.IntegrationEvents.Products;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Demo.Architecture.UseCases.DomainEventHandlers.Products;

public class ProductCreatedDomainEventHandler(
    ILogger<ProductCreatedDomainEventHandler> logger,
    IIntegrationEventPublisher publisher)
    : INotificationHandler<ProductCreatedDomainEvent>
{
    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var traceId = Guid.Parse(Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString());

        logger.LogInformation(
            "[TraceId: {TraceId}] Product created: {ProductId} - {ProductName}",
            traceId,
            notification.Product.Id,
            notification.Product.Name);

        // 1️. Publish event (broadcast)
        var integrationEvent = new ProductCreatedIntegrationEvent
        {
            Id = notification.Product.Id.Value,
            Name = notification.Product.Name,
            Price = notification.Product.Price.Value
        };

        await publisher.PublishAsync(integrationEvent, traceId, cancellationToken);

        // 2️. Send command (targeted)
        var command = new InitializeInventoryCommand
        {
            ProductId = notification.Product.Id.Value
        };

        await publisher.SendAsync(command, "inventory-command", cancellationToken);

        logger.LogInformation(
           "[ProductCreatedDomainEvent] Published event & sent command for ProductId: {ProductId}",
           notification.Product.Id);
    }
}
