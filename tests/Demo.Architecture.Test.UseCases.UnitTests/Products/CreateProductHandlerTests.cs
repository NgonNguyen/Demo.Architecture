using Ardalis.Result;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Features.Products.Commands.Create;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUlid;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
public class CreateProductHandlerTests : TestBase
{
    private CreateProductCommandHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _handler = new CreateProductCommandHandler(Context);
    }

    [Test]
    public async Task Should_Create_Product_And_Save()
    {
        // Arrange
        var command = new CreateProductCommand(TestConstants.ValidProductNameA, TestConstants.ValidPriceA);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(default(Ulid));

        var saved = await Context.Products.FirstAsync();
        saved.Name.Should().Be(TestConstants.ValidProductNameA);
        saved.Price.Value.Should().Be(TestConstants.ValidPriceA);
        saved.Id.Value.Should().Be(result.Value);
    }

    [Test]
    public async Task Should_Return_Invalid_When_Domain_Fails()
    {
        // Arrange: invalid name and price
        var command = new CreateProductCommand(string.Empty, 0);

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
}
