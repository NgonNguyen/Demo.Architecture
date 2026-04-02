using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Features.Products.Commands.Delete;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
public class DeleteProductHandlerTests : TestBase
{
    private DeleteProductCommandHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _handler = new DeleteProductCommandHandler(Context);
    }

    [Test]
    public async Task Should_Deactivate_Product_When_Exists()
    {
        // Arrange
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var command = new DeleteProductCommand(product.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);

        var deactivated = await Context.Products.FindAsync(product.Id);
        deactivated.Should().NotBeNull();
        deactivated!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId.New().Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Is_Already_Inactive()
    {
        // Arrange
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        product.Deactivate();
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var command = new DeleteProductCommand(product.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Test]
    public async Task Should_Not_Delete_Other_Products()
    {
        // Arrange
        var product1 = Product.Create("Product 1", 100).Value;
        var product2 = Product.Create("Product 2", 200).Value;

        Context.Products.Add(product1);
        Context.Products.Add(product2);
        await Context.SaveChangesAsync();

        var command = new DeleteProductCommand(product1.Id.Value);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var remaining = await Context.Products.FindAsync(product2.Id);
        remaining.Should().NotBeNull();
        remaining!.IsActive.Should().BeTrue();
    }
    //    var result = await _handler.Handle(command, CancellationToken.None);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();

    //    Context.Products.Received(1).Remove(product);

    //    await Context.Received(1)
    //        .SaveChangesAsync(Arg.Any<CancellationToken>());
    //}
}
