using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Helpers.Products;
using Demo.Architecture.Test.Shared.Web;
using Demo.Architecture.UseCases.Features.Products.Commands.Delete;
using Demo.Architecture.WebAPI.Features.Products.Delete;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Net;

namespace Demo.Architecture.Test.WebAPI.IntegrationTests.Products;

[TestFixture]
public class DeleteProductEndpointTests
{
    // ---------------- BASIC (unit) ----------------

    [Test]
    public async Task Should_Return_NoContent_When_Delete_Succeeds()
    {
        // Arrange
        var id = Ulid.NewUlid();
        var command = new DeleteProductCommand(id);

        var senderMock = new Mock<ISender>();
        senderMock
            .Setup(x => x.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await DeleteProductEndpoint.Handle(senderMock.Object, id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task Should_Call_MediatR_With_Correct_Command()
    {
        // Arrange
        var id = Ulid.NewUlid();
        var command = new DeleteProductCommand(id);

        var senderMock = new Mock<ISender>();
        senderMock
            .Setup(x => x.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await DeleteProductEndpoint.Handle(senderMock.Object, id, CancellationToken.None);

        // Assert
        senderMock.Verify(
            x => x.Send(It.Is<DeleteProductCommand>(cmd => cmd.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var id = Ulid.NewUlid();

        var senderMock = new Mock<ISender>();
        senderMock
            .Setup(x => x.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        // Act
        var result = await DeleteProductEndpoint.Handle(senderMock.Object, id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    // ---------------- HTTP Client ----------------

    [Test]
    public async Task Should_Return_204_When_Valid()
    {
        // Arrange - create product first
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product = Product.Create("Product 1", 100).Value;

        db.Products.Add(product);
        db.SaveChanges();

        // Act
        var response = await client.SendAsync(ProductTestDataHelper.DeleteRequest(product.Id.Value));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Should_Return_404_When_Product_Not_Found()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var randomId = Ulid.NewUlid();

        // Act
        var response = await client.SendAsync(ProductTestDataHelper.DeleteRequest(randomId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Should_Not_Delete_Other_Products()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product1 = Product.Create("Product 1", 100).Value;
        var product2 = Product.Create("Product 2", 200).Value;

        db.Products.Add(product1);
        db.Products.Add(product2);
        db.SaveChanges();

        // Act
        await client.SendAsync(ProductTestDataHelper.DeleteRequest(product1.Id.Value));

        // Assert
        var remaining = await db.Products.FindAsync(product2.Id);
        remaining.Should().NotBeNull();
        remaining!.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task Should_Return_404_When_Trying_Delete_Already_Deleted_Product()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var productId = ProductId.New();
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        // Set the product ID explicitly
        typeof(Product)
            .GetProperty(nameof(Product.Id))!
            .SetValue(product, productId);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Products.Add(product);
            await db.SaveChangesAsync();
        }

        // Delete once 
        var firstResponse = await client.SendAsync(ProductTestDataHelper.DeleteRequest(productId.Value));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Try to delete again
        var response = await client.SendAsync(ProductTestDataHelper.DeleteRequest(productId.Value));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------- Idempotency ----------------

    [Test]
    public async Task Should_Return_Same_Response_When_Same_IdempotencyKey_And_Request()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product = Product.Create("Product 1", 100).Value;

        db.Products.Add(product);
        db.SaveChanges();

        var idempotencyKey = Ulid.NewUlid().ToString();

        // First request
        var first = await client.SendAsync(
            ProductTestDataHelper.DeleteRequest(product.Id.Value, idempotencyKey));

        // Second request (same key + same payload)
        var second = await client.SendAsync(
            ProductTestDataHelper.DeleteRequest(product.Id.Value, idempotencyKey));

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
