using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Commands.Delete;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

public class DeleteProductHandlerMockTests
{
    private Mock<IApplicationDbContext> _writeContextMock = default!;
    private DeleteProductCommandHandler _handler = default!;
    private List<Product> _products = default!;

    [SetUp]
    public void Setup()
    {
        _products = new List<Product>();
        var mockDbSet = _products.BuildMockDbSet();

        _writeContextMock = new Mock<IApplicationDbContext>();
        _writeContextMock.Setup(c => c.Products).Returns(mockDbSet.Object);
        _writeContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new DeleteProductCommandHandler(_writeContextMock.Object);
    }

    [Test]
    public async Task Should_Deactivate_Product_When_Exists()
    {
        var product = Product.Create("Product A", 100).Value;
        _products.Add(product);

        var command = new DeleteProductCommand(product.Id.Value);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Ok);

        product.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var command = new DeleteProductCommand(ProductId.New().Value);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Test]
    public async Task Should_Not_Delete_Other_Products()
    {
        var product1 = Product.Create("Product 1", 100).Value;
        var product2 = Product.Create("Product 2", 200).Value;
        _products.Add(product1);
        _products.Add(product2);

        var command = new DeleteProductCommand(product1.Id.Value);

        await _handler.Handle(command, CancellationToken.None);

        product2.IsActive.Should().BeTrue();
        product1.IsActive.Should().BeFalse();
    }
}
