using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.DomainEventHandlers.Products;
using Demo.Architecture.UseCases.IntegrationEvents.Products;
using Microsoft.Extensions.Logging;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Diagnostics;

namespace Demo.Architecture.Test.UseCases.UnitTests.DomainEventHandlers.Products;

public class ProductUpdatedDomainEventHandlerTests
{
    private Mock<IIntegrationEventPublisher> _publisher = default!;
    private Mock<ILogger<ProductUpdatedDomainEventHandler>> _logger = default!;
    private ProductUpdatedDomainEventHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _publisher = new Mock<IIntegrationEventPublisher>();
        _logger = new Mock<ILogger<ProductUpdatedDomainEventHandler>>();

        _handler = new ProductUpdatedDomainEventHandler(
            _logger.Object,
            _publisher.Object
        );
    }

    [Test]
    public async Task Should_Publish_ProductUpdatedIntegrationEvent()
    {
        // Arrange
        using var activity = new Activity("Test");
        activity.Start();
        var expectedTraceId = Guid.Parse(activity.TraceId.ToString());

        var domainEvent = new ProductUpdatedDomainEvent(
            productId: Ulid.NewUlid(),
            name: TestConstants.ValidProductNameA,
            price: TestConstants.ValidPriceA
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _publisher.Verify(x =>
            x.PublishAsync(
                It.Is<ProductUpdatedIntegrationEvent>(e =>
                    e.Id == domainEvent.ProductId &&
                    e.Name == domainEvent.Name &&
                    e.Price == domainEvent.Price
                ),
                expectedTraceId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
