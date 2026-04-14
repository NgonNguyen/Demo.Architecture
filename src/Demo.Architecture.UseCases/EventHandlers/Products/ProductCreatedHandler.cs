using Demo.Architecture.Core.Events.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Architecture.UseCases.EventHandlers.Products;

public class ProductCreatedHandler(
    ILogger<ProductCreatedHandler> logger)
    : INotificationHandler<ProductCreatedDomainEvent>
{
    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ProductCreatedDomainEvent]  Product created: {ProductId} - {ProductName}", notification.Product.Id, notification.Product.Name);
        return Task.CompletedTask;
    }
}
