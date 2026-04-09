using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Commands.Update;
using Demo.Architecture.UseCases.Features.Products.Rules;
using FluentAssertions;
using FluentValidation.TestHelper;
using MockQueryable.Moq;
using Moq;
using NUlid;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

public class UpdateProductHandlerMockTests
{
    private Mock<IProductUniquenessChecker> _checkerMock = default!;
    private Mock<IApplicationDbContext> _writeContextMock = default!;
    private UpdateProductCommandValidator _validator = default!;
    private UpdateProductCommandHandler _handler = default!;
    private List<Product> _products = default!;

    [SetUp]
    public void Setup()
    {
        _checkerMock = new Mock<IProductUniquenessChecker>();
        _writeContextMock = new Mock<IApplicationDbContext>();

        _products = new List<Product>();
        var mockDbSet = _products.BuildMockDbSet();
        _writeContextMock.Setup(c => c.Products).Returns(mockDbSet.Object);
        _writeContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _validator = new UpdateProductCommandValidator(_checkerMock.Object);
        _handler = new UpdateProductCommandHandler(_writeContextMock.Object);
    }

    // ---------------- Validation ----------------

    [Test]
    public async Task Should_Pass_When_Name_Is_Unique()
    {
        _checkerMock.Setup(c => c.IsNameUnique("UniqueName", It.IsAny<Ulid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateProductCommand(Ulid.NewUlid(), "UniqueName", 100);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public async Task Should_Fail_When_Name_Already_Exists()
    {
        _checkerMock.Setup(c => c.IsNameUnique("ExistingName", It.IsAny<Ulid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateProductCommand(Ulid.NewUlid(), "ExistingName", 100);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
              .WithErrorMessage(ProductErrors.DuplicatedName.Message)
              .WithErrorCode(ProductErrors.DuplicatedName.Code);
    }

    [Test]
    public async Task Should_Fail_When_Name_Is_Empty()
    {
        var command = new UpdateProductCommand(Ulid.NewUlid(), string.Empty, 100);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
              .WithErrorMessage(ProductErrors.NameRequired.Message)
              .WithErrorCode(ProductErrors.NameRequired.Code);
    }

    [Test]
    public async Task Should_Fail_When_Price_Is_Invalid()
    {
        var command = new UpdateProductCommand(Ulid.NewUlid(), "ValidName", 0);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Price)
              .WithErrorMessage(ProductErrors.PriceInvalid.Message)
              .WithErrorCode(ProductErrors.PriceInvalid.Code);
    }

    // ---------------- Handler ----------------

    [Test]
    public async Task Should_Return_Invalid_When_Domain_Fails()
    {
        var product = Product.Create("Product A", 100).Value;
        _products.Add(product);

        var command = new UpdateProductCommand(product.Id.Value, string.Empty, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == ProductErrors.NameRequired.Code);
        result.ValidationErrors.Should().Contain(e => e.Identifier == ProductErrors.PriceInvalid.Code);
    }

    [Test]
    public async Task Should_Update_Product_And_Save()
    {
        var product = Product.Create("Product A", 100).Value;
        _products.Add(product);

        var command = new UpdateProductCommand(product.Id.Value, "Product B", 200);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        product.Name.Should().Be("Product B");
        product.Price.Value.Should().Be(200);
        _writeContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Not_Exists()
    {
        var command = new UpdateProductCommand(Ulid.NewUlid(), "Name", 100);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
        result.Errors.Should().Contain(ProductErrors.NotFound.Code);
    }
}
