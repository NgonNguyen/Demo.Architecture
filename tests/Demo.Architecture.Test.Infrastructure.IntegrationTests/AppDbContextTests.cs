using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.Infrastructure.IntegrationTests;

[TestFixture]
public class AppDbContextTests : TestBase
{
    [Test]
    public async Task SaveChangesAsync_Should_Publish_ProductCreatedDomainEvent()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Assert
        MediatorMock.Verify(m =>
            m.Publish(
                It.Is<ProductCreatedDomainEvent>(e => e.Product == product),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SaveChangesAsync_Should_Publish_ProductUpdatedDomainEvent()
    {
        // Arrange
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Act
        product.Update(TestConstants.ValidProductNameB, TestConstants.ValidPriceB);
        await Context.SaveChangesAsync();

        // Assert
        MediatorMock.Verify(m =>
            m.Publish(
                It.Is<ProductUpdatedDomainEvent>(e =>
                    e.ProductId == product.Id.Value &&
                    e.Name == TestConstants.ValidProductNameB &&
                    e.Price == TestConstants.ValidPriceB),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
