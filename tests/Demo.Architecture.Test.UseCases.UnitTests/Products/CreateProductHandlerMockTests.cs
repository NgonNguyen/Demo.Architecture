using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Commands.Create;
using Demo.Architecture.UseCases.Features.Products.Rules;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Products;

public class CreateProductHandlerMockTests
{
    private Mock<IProductUniquenessChecker> _checkerMock = default!;
    private Mock<IApplicationDbContext> _writeContextMock = default!;
    private CreateProductCommandValidator _validator = default!;
    private CreateProductCommandHandler _handler = default!;

    [SetUp]
    public void Setup()
    {
        _checkerMock = new Mock<IProductUniquenessChecker>();
        _writeContextMock = new Mock<IApplicationDbContext>();

        _validator = new CreateProductCommandValidator(_checkerMock.Object);
        _handler = new CreateProductCommandHandler(_writeContextMock.Object);
    }

    // ---------------- Validation ----------------

    [Test]
    public async Task Should_Pass_When_Name_Is_Unique()
    {
        _checkerMock.Setup(c => c.IsNameUnique("UniqueName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateProductCommand("UniqueName", 100);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }

    [Test]
    public async Task Should_Fail_When_Name_Already_Exists()
    {
        _checkerMock.Setup(c => c.IsNameUnique("ExistingName", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateProductCommand("ExistingName", 100);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
              .WithErrorMessage(ProductErrors.DuplicatedName.Message)
              .WithErrorCode(ProductErrors.DuplicatedName.Code);
    }

    [Test]
    public async Task Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateProductCommand(string.Empty, 100);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
              .WithErrorMessage(ProductErrors.NameRequired.Message)
              .WithErrorCode(ProductErrors.NameRequired.Code);
    }

    [Test]
    public async Task Should_Fail_When_Price_Is_Invalid()
    {
        var command = new CreateProductCommand("ValidName", 0);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(c => c.Price)
              .WithErrorMessage(ProductErrors.PriceInvalid.Message)
              .WithErrorCode(ProductErrors.PriceInvalid.Code);
    }

    // ---------------- Handler ----------------

    [Test]
    public async Task Should_Return_Invalid_When_Domain_Fails()
    {
        var command = new CreateProductCommand(string.Empty, 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == ProductErrors.NameRequired.Code);
        result.ValidationErrors.Should().Contain(e => e.Identifier == ProductErrors.PriceInvalid.Code);
    }

    [Test]
    public async Task Should_Create_Product_And_Save()
    {
        var command = new CreateProductCommand("ValidName", 100);

        // Arrange: mock DbSet and SaveChanges
        var mockDbSet = new Mock<DbSet<Product>>();
        _writeContextMock.Setup(c => c.Products).Returns(mockDbSet.Object);
        _writeContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(default);

        // Verify that Add and SaveChanges were called
        mockDbSet.Verify(d => d.Add(It.IsAny<Product>()), Times.Once);
        _writeContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
