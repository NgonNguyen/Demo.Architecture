using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.Test.Shared.Builders;
using Demo.Architecture.Test.Shared.Constants;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Architecture.Test.Domain.UnitTests.Products;

[TestFixture]
public class ProductTests
{
    // ---------------- CREATE ----------------

    [Test]
    public void Create_Should_Succeed_When_Input_Is_Valid()
    {
        // Act
        var result = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(TestConstants.ValidProductNameA);
        result.Value.Price.Value.Should().Be(TestConstants.ValidPriceA);
        result.Value.Id.Should().NotBeNull();
    }

    [Test]
    public void Create_Should_Fail_When_Name_Is_Empty()
    {
        // Act
        var result = Product.Create(TestConstants.InvalidProductName, TestConstants.ValidPriceA);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.ValidationErrors.Should().ContainSingle(e =>
           e.Identifier == ProductErrors.NameRequired.Code);
        result.ValidationErrors.Should().ContainSingle(e =>
            e.ErrorMessage == ProductErrors.NameRequired.Message);
    }

    [Test]
    public void Create_Should_Fail_When_Price_Is_Invalid()
    {
        // Act
        var result = Product.Create(TestConstants.ValidProductNameA, TestConstants.InvalidPrice);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.ValidationErrors.Should().ContainSingle(e =>
           e.Identifier == ProductErrors.PriceInvalid.Code);
        result.ValidationErrors.Should().ContainSingle(e =>
            e.ErrorMessage == ProductErrors.PriceInvalid.Message);
    }

    [Test]
    public void Create_Should_Return_Multiple_Errors_When_Input_Is_Invalid()
    {
        // Act
        var result = Product.Create(TestConstants.InvalidProductName, TestConstants.InvalidPrice);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.ValidationErrors.Should().HaveCount(2);

        result.ValidationErrors.Should().ContainSingle(e =>
           e.Identifier == ProductErrors.NameRequired.Code);
        result.ValidationErrors.Should().Contain(e =>
            e.ErrorMessage == ProductErrors.NameRequired.Message);

        result.ValidationErrors.Should().ContainSingle(e =>
           e.Identifier == ProductErrors.PriceInvalid.Code);
        result.ValidationErrors.Should().Contain(e =>
            e.ErrorMessage == ProductErrors.PriceInvalid.Message);
    }

    // ---------------- UPDATE ----------------

    [Test]
    public void Update_Should_Succeed_When_Input_Is_Valid()
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        var result = product.Update(TestConstants.ValidProductNameB, TestConstants.ValidPriceB);

        // Assert
        result.IsSuccess.Should().BeTrue();

        product.Name.Should().Be(TestConstants.ValidProductNameB);
        product.Price.Value.Should().Be(TestConstants.ValidPriceB);
    }

    [Test]
    public void Update_Should_Fail_When_Name_Is_Empty()
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        var result = product.Update(TestConstants.InvalidProductName, TestConstants.ValidPriceB);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.ValidationErrors.Should().ContainSingle(e =>
          e.Identifier == ProductErrors.NameRequired.Code);
        result.ValidationErrors.Should().ContainSingle(e =>
            e.ErrorMessage == ProductErrors.NameRequired.Message);
    }

    [Test]
    public void Update_Should_Fail_When_Price_Is_Invalid()
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        var result = product.Update(TestConstants.ValidProductNameB, TestConstants.InvalidPrice);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.ValidationErrors.Should().ContainSingle(e =>
          e.Identifier == ProductErrors.PriceInvalid.Code);
        result.ValidationErrors.Should().ContainSingle(e =>
            e.ErrorMessage == ProductErrors.PriceInvalid.Message);
    }

    [Test]
    public void Update_Should_Return_Multiple_Errors_When_Input_Is_Invalid()
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        var result = product.Update(TestConstants.InvalidProductName, TestConstants.InvalidPrice);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.ValidationErrors.Should().HaveCount(2);

        result.ValidationErrors.Should().ContainSingle(e =>
           e.Identifier == ProductErrors.NameRequired.Code);
        result.ValidationErrors.Should().Contain(e =>
            e.ErrorMessage == ProductErrors.NameRequired.Message);

        result.ValidationErrors.Should().ContainSingle(e =>
           e.Identifier == ProductErrors.PriceInvalid.Code);
        result.ValidationErrors.Should().Contain(e =>
            e.ErrorMessage == ProductErrors.PriceInvalid.Message);
    }

    [Test]
    public void Update_Should_Not_Modify_State_When_Validation_Fails()
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        var result = product.Update(TestConstants.InvalidProductName, TestConstants.InvalidPrice);

        // Assert
        result.IsSuccess.Should().BeFalse();

        product.Name.Should().Be(TestConstants.ValidProductNameA);
        product.Price.Value.Should().Be(TestConstants.ValidPriceA);
    }
}
