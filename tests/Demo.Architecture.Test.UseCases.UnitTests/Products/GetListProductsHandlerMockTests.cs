using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
public class GetListProductsHandlerMockTests
{
    private Mock<IReadOnlyApplicationDbContext> _mockContext = default!;
    private GetListProductsHandler _handler = default!;
    private List<Product> _products = default!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IReadOnlyApplicationDbContext>();
        _products = new List<Product>
        {
            Product.Create("Product A", 10).Value,
            Product.Create("Product B", 20).Value,
            Product.Create("Product C", 30).Value
        };
        var mockDbSet = _products.BuildMockDbSet();
        _mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);

        _handler = new GetListProductsHandler(_mockContext.Object);
    }

    // ---------------- BASIC ----------------

    [Test]
    public async Task Should_Return_All_Active_Products()
    {
        var query = new GetListProductsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
    }

    // ---------------- SEARCH ----------------
    [Test]
    public async Task Should_Filter_By_SearchTerm()
    {
        var query = new GetListProductsQuery { SearchTerm = "Product A" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Product A");
    }

    // ---------------- SORT ----------------
    [Test]
    public async Task Should_Sort_By_Name()
    {
        var query = new GetListProductsQuery { Sort = "name" };

        var result = await _handler.Handle(query, CancellationToken.None);

        var names = result.Value.Items.Select(x => x.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Should_Sort_By_Price()
    {
        var query = new GetListProductsQuery { Sort = "price" };

        var result = await _handler.Handle(query, CancellationToken.None);

        var prices = result.Value.Items.Select(x => x.Price).ToList();
        prices.Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Should_Sort_By_Price_Descending()
    {
        var query = new GetListProductsQuery { Sort = "price_desc" };

        var result = await _handler.Handle(query, CancellationToken.None);

        var prices = result.Value.Items.Select(x => x.Price).ToList();
        prices.Should().BeInDescendingOrder();
    }

    // ---------------- PAGINATION ----------------
    [Test]
    public async Task Should_Apply_Pagination()
    {
        var query = new GetListProductsQuery { Page = 2, PageSize = 2 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Page.Should().Be(2);
    }

    // ---------------- EMPTY ----------------
    [Test]
    public async Task Should_Return_Empty_When_No_Data()
    {
        _products.Clear();
        var mockDbSet = _products.BuildMockDbSet();
        _mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);

        var query = new GetListProductsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
