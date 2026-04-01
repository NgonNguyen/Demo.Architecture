using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Features.Products.Commands.Update;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUlid;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
internal class UpdateProductHandlerTests : TestBase
{
    private UpdateProductCommandHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _handler = new UpdateProductCommandHandler(Context);
    }

    [Test]
    public async Task Should_Update_Product_And_Save()
    {
        // Arrange
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var command = new UpdateProductCommand(product.Id.Value, TestConstants.ValidProductNameB, TestConstants.ValidPriceB);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(default(Ulid));

        var saved = await Context.Products.FirstAsync();
        saved.Name.Should().Be(TestConstants.ValidProductNameB);
        saved.Price.Value.Should().Be(TestConstants.ValidPriceB);
    }

    [Test]
    public async Task Should_Return_Invalid_When_Domain_Fails()
    {
        // Arrange: invalid name and price
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var command = new UpdateProductCommand(product.Id.Value, string.Empty, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().NotBeEmpty();
        result.ValidationErrors.Select(e => e.Identifier).Should().Contain(ProductErrors.NameRequired.Code);
        result.ValidationErrors.Select(e => e.ErrorMessage).Should().Contain(ProductErrors.NameRequired.Message);
        result.ValidationErrors.Select(e => e.Identifier).Should().Contain(ProductErrors.PriceInvalid.Code);
        result.ValidationErrors.Select(e => e.ErrorMessage).Should().Contain(ProductErrors.PriceInvalid.Message);
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Not_Exists()
    {
        // Arrange: invalid name and price
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var command = new UpdateProductCommand(Ulid.NewUlid(), string.Empty, 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(ProductErrors.NotFound.Code);
    }
}
