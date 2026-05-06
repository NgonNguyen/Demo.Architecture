using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.DomainEventHandlers.Products;
using Demo.Architecture.UseCases.IntegrationEvents.Products;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Diagnostics;

namespace Demo.Architecture.Test.UseCases.UnitTests.DomainEventHandlers.Products;

public class ProductCreatedDomainEventHandlerTests
{
    private readonly Mock<IIntegrationEventPublisher> _publisher = new();
    private readonly Mock<ILogger<ProductCreatedDomainEventHandler>> _logger = new();

    private readonly ProductCreatedDomainEventHandler _handler;

    public ProductCreatedDomainEventHandlerTests()
    {
        _handler = new ProductCreatedDomainEventHandler(
            _logger.Object,
            _publisher.Object
        );
    }

    [Test]
    public async Task Should_Publish_ProductCreatedIntegrationEvent()
    {
        // Arrange
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        using var activity = new Activity("Test");
        activity.Start();
        var expectedTraceId = Guid.Parse(activity.TraceId.ToString());
        var domainEvent = new ProductCreatedDomainEvent(product);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _publisher.Verify(x =>
            x.PublishAsync(
                It.Is<ProductCreatedIntegrationEvent>(e =>
                    e.Id == product.Id.Value &&
                    e.Name == product.Name &&
                    e.Price == product.Price.Value
                ),
                expectedTraceId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _publisher.Verify(p => p.SendAsync(
           It.Is<InitializeInventoryCommand>(c =>
               c.ProductId == product.Id.Value),
           "inventory-command",
           It.IsAny<CancellationToken>()),
           Times.Once);
    }
}
