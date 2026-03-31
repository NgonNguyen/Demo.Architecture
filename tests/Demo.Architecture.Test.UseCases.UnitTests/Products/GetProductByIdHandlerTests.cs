using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Features.Products.Queries.GetById;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
public class GetProductByIdHandlerTests : TestBase
{
    private GetProductByIdHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _handler = new GetProductByIdHandler(Context);
    }

    [Test]
    public async Task Should_Return_Product_When_Exists()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var query = new GetProductByIdQuery(product.Id);
        
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(product.Id.Value);
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Not_Exists()
    {
        var query = new GetProductByIdQuery(ProductId.New());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
