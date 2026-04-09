using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Queries.GetById;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

[TestFixture]
public class GetProductByIdHandlerMockTests
{
    private Mock<IReadOnlyApplicationDbContext> _mockContext = default!;
    private GetProductByIdHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IReadOnlyApplicationDbContext>();
        _handler = new GetProductByIdHandler(_mockContext.Object);
    }

    [Test]
    public async Task Should_Return_Product_When_Exists()
    {
        var product = Product.Create("TestProduct", 100).Value;
        var products = new List<Product> { product };

        var mockDbSet = products.BuildMockDbSet();

        var mockContext = new Mock<IReadOnlyApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);

        var handler = new GetProductByIdHandler(mockContext.Object);
        var query = new GetProductByIdQuery(product.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(product.Id.Value);
        result.Value!.Name.Should().Be(product.Name);
        result.Value!.Price.Should().Be(product.Price.Value);
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Not_Exists()
    {
        // Arrange: empty product list
        var products = new List<Product>();
        var mockDbSet = products.BuildMockDbSet();
        _mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);

        var query = new GetProductByIdQuery(ProductId.New());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
        result.Errors.Should().Contain(ProductErrors.NotFound.Code);
        result.Value.Should().BeNull();
    }
}