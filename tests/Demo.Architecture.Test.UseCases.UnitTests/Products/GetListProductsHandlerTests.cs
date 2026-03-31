using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Seeders;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
public class GetListProductsHandlerTests : TestBase
{
    private GetListProductsHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _handler = new GetListProductsHandler(Context);
    }

    // ---------------- BASIC ----------------

    [Test]
    public async Task Should_Return_All_Active_Products()
    {
        await ProductSeeder.SeedAsync(Context);

        var query = new GetListProductsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
    }

    // ---------------- SEARCH ----------------

    [Test]
    public async Task Should_Filter_By_SearchTerm()
    {
        await ProductSeeder.SeedAsync(Context);

        var query = new GetListProductsQuery
        {
            SearchTerm = "Product A"
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Product A");
    }

    // ---------------- SORT ----------------

    [Test]
    public async Task Should_Sort_By_Name()
    {
        await ProductSeeder.SeedAsync(Context);

        var query = new GetListProductsQuery
        {
            Sort = "name"
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        var names = result.Value.Items.Select(x => x.Name).ToList();

        names.Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Should_Sort_By_Price()
    {
        await ProductSeeder.SeedAsync(Context);

        var query = new GetListProductsQuery
        {
            Sort = "price"
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        var prices = result.Value.Items.Select(x => x.Price).ToList();

        prices.Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Should_Sort_By_Price_Descending()
    {
        await ProductSeeder.SeedAsync(Context);

        var query = new GetListProductsQuery
        {
            Sort = "price_desc"
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        var prices = result.Value.Items.Select(x => x.Price).ToList();

        prices.Should().BeInDescendingOrder();
    }

    // ---------------- PAGINATION ----------------

    [Test]
    public async Task Should_Apply_Pagination()
    {
        await ProductSeeder.SeedAsync(Context);

        var query = new GetListProductsQuery
        {
            Page = 2,
            PageSize = 2
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Page.Should().Be(2);
    }

    // ---------------- INACTIVE ----------------

    [Test]
    public async Task Should_Exclude_Inactive_Products()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        product.Deactivate();

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var query = new GetListProductsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items.Should().BeEmpty();
    }

    // ---------------- EMPTY ----------------

    [Test]
    public async Task Should_Return_Empty_When_No_Data()
    {
        var query = new GetListProductsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
