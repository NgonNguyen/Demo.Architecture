using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Test.Shared;
using Demo.Architecture.Test.Shared.Constants;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Demo.Architecture.Test.Infrastructure.IntegrationTests.Products;

[TestFixture]
public class ProductTests : TestBase
{
    // ---------------- PERSISTENCE ----------------

    [Test]
    public async Task Can_Insert_And_Retrieve()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var savedProduct = await Context.Products.FirstAsync(x => x.Id == product.Id);

        savedProduct.Should().NotBeNull();
        savedProduct.Name.Should().Be(TestConstants.ValidProductNameA);
        savedProduct.Price.Value.Should().Be(TestConstants.ValidPriceA);
    }

    [Test]
    public async Task Should_Get_Product_By_Id_From_Database()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        var found = await Context.Products.FirstAsync(p => p.Id == product.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(product.Id);
        found.Name.Should().Be(TestConstants.ValidProductNameA);
        found.Price.Value.Should().Be(TestConstants.ValidPriceA);
    }

    [Test]
    public async Task Should_Return_Paged_Products()
    {
        // Seed 25 products
        for (int i = 0; i < 25; i++)
        {
            Context.Products.Add(Product.Create($"Product{i:D2}", 100 + i).Value);
        }

        await Context.SaveChangesAsync();

        var page = 2;
        var pageSize = 10;

        var items = await Context.Products
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await Context.Products.CountAsync();

        items.Should().HaveCount(pageSize);
        total.Should().Be(25);
    }

    // ---------------- CONVERSIONS ----------------

    [Test]
    public void Should_Persist_ProductId_With_Ulid_Conversion()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        Context.Products.Add(product);
        Context.SaveChanges();

        var retrieved = Context.Products.AsNoTracking().First();

        retrieved.Id.Should().Be(product.Id);
    }

    [Test]
    public void Should_Persist_Money_With_Decimal_Conversion()
    {
        var product = Product.Create(TestConstants.ValidProductNameB, TestConstants.ValidPriceB).Value;

        Context.Products.Add(product);
        Context.SaveChanges();

        var retrieved = Context.Products.First();

        retrieved.Price.Value.Should().Be(TestConstants.ValidPriceB);
    }

    // ---------------- CONSTRAINTS ----------------

    [Test]
    public void Should_Throw_When_Name_Is_Null()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        typeof(Product)
            .GetProperty(nameof(Product.Name))!
            .SetValue(product, null);

        Context.Products.Add(product);

        Action act = () => Context.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }

    [Test]
    public void Should_Configure_Name_MaxLength_256() // SQLite not support maxlength
    {
        var entityType = Context.Model.FindEntityType(typeof(Product));
        var property = entityType.FindProperty(nameof(Product.Name));

        property.GetMaxLength().Should().Be(256);
        property.IsNullable.Should().BeFalse();
    }

    /* SQLServer or other support maxlength
    [Test]
    public void Should_Throw_When_Name_Exceeds_MaxLength()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;

        typeof(Product)
            .GetProperty(nameof(Product.Name))!
            .SetValue(product, TestConstants.MaxProductName);

        Context.Products.Add(product);

        Action act = () => Context.SaveChanges();

        act.Should().Throw<DbUpdateException>();
    }
    */

    // ---------------- DECIMAL PRECISION ----------------

    [Test]
    public void Should_Configure_Price_Decimal() // SQLite not support decimal precision
    {
        var entityType = Context.Model.FindEntityType(typeof(Product));
        var property = entityType.FindProperty(nameof(Product.Price));

        property.GetColumnType().Should().Be("decimal(18,2)");
        property.IsNullable.Should().BeFalse();
    }

    /* SQLServer or other support decimal precision
    [Test]
    public void Should_Round_Price_To_2_Decimal_Places()
    {
        var product = Product.Create(TestConstants.ValidProductNameA, 123.456m).Value;

        Context.Products.Add(product);
        Context.SaveChanges();

        var retrieved = Context.Products.First();

        retrieved.Price.Value.Should().Be(123.46m);
    }
    */
}
