using Demo.Architecture.Core.Events.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Demo.Architecture.UseCases.EventHandlers.Products;

public class ProductUpdatedHandler(
     ILogger<ProductUpdatedHandler> logger)
    : INotificationHandler<ProductUpdatedDomainEvent>
{
    public Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ProductUpdatedDomainEvent] Product with ID {ProductId} has been updated. Name: {Name}, Price: {Price}",
            notification.ProductId, notification.Name, notification.Price);
        return Task.CompletedTask;
    }
}
