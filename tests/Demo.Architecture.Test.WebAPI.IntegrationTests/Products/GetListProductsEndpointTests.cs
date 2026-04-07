using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Helpers;
using Demo.Architecture.Test.Shared.Helpers.Products;
using Demo.Architecture.Test.Shared.Seeders;
using Demo.Architecture.Test.Shared.Web;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using Demo.Architecture.WebAPI.Features.Products.GetList;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.Test.WebAPI.IntegrationTests.Products;

[TestFixture]
public class GetListProductsEndpointTests
{
    // ---------------- BASIC ----------------

    [Test]
    public async Task Should_Return_Ok_When_Query_Succeeds()
    {
        // Arrange
        var query = new GetListProductsQuery
        {
            Page = 1,
            PageSize = 10
        };

        var response = ProductTestDataHelper.CreatePagedResponse();

        var senderMock = SenderMockHelper.CreateSuccess<
            GetListProductsQuery,
            AppModels.PagedResult<GetListProductsResponse>>(query, response);

        // Act
        var result = await GetAllProductsEndpoint.Handle(
            senderMock.Object,
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Ok<AppModels.PagedResult<GetListProductsResponse>>>();
    }

    [Test]
    public async Task Should_Call_MediatR_With_Correct_Query()
    {
        // Arrange
        var query = new GetListProductsQuery();

        var response = ProductTestDataHelper.CreatePagedResponse();

        var senderMock = new Mock<ISender>();

        senderMock
            .Setup(x => x.Send(It.IsAny<GetListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        // Act
        await GetAllProductsEndpoint.Handle(
            senderMock.Object,
            query,
            CancellationToken.None);

        // Assert
        senderMock.Verify(x =>
            x.Send(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Should_Return_Problem_When_Result_Fails()
    {
        // Arrange
        var query = new GetListProductsQuery();

        var senderMock = SenderMockHelper.CreateFailure<
            GetListProductsQuery,
            AppModels.PagedResult<GetListProductsResponse>>(
            query,
            new List<ValidationError>
            {
                new() { ErrorMessage = "Something went wrong" }
            });

        // Act
        var result = await GetAllProductsEndpoint.Handle(
            senderMock.Object,
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    // ---------------- HTTP Client ----------------

    [Test]
    public async Task Should_Return_Products()
    {
        // Arrange
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // 🔥 Seed using REAL app container
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value);
        db.Products.Add(Product.Create(TestConstants.ValidProductNameB, TestConstants.ValidPriceA).Value);
        db.SaveChanges();

        // Act
        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_Return_Empty_When_No_Products()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>();

        result!.Items.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Respect_PageSize()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (int i = 0; i < 20; i++)
        {
            db.Products.Add(Product.Create($"Product{i}", 100).Value);
        }
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Should().HaveCount(5);
    }

    [Test]
    public async Task Should_Return_Second_Page()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (int i = 0; i < 10; i++)
        {
            db.Products.Add(Product.Create($"Product{i}", 100).Value);
        }
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=2&pageSize=5");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Should().HaveCount(5);
    }

    [Test]
    public async Task Should_Return_Correct_Product_Data()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=10");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        var product = result!.Items.First();

        product.Name.Should().Be(TestConstants.ValidProductNameA);
        product.Price.Should().Be(TestConstants.ValidPriceA);
    }

    [Test]
    public async Task Should_Filter_By_SearchTerm()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("Laptop Dell", 1000).Value);
        db.Products.Add(Product.Create("iPhone", 500).Value);
        db.Products.Add(Product.Create("Laptop HP", 900).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&searchTerm=laptop");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Should().HaveCount(2);
    }

    [Test]
    public async Task Should_Be_Case_Insensitive()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("Laptop", 1000).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&searchTerm=LAPTOP");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task Should_Return_Empty_When_No_Match()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("Laptop", 1000).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&searchTerm=xyz");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Should().BeEmpty();
    }

    [Test]
    public async Task Should_Order_By_Name_Ascending()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("B Product", 100).Value);
        db.Products.Add(Product.Create("A Product", 100).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&sort=name");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Select(x => x.Name)
            .Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Should_Order_By_Name_Descending()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("A Product", 100).Value);
        db.Products.Add(Product.Create("B Product", 100).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&sort=name_desc");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Select(x => x.Name)
            .Should().BeInDescendingOrder();
    }

    [Test]
    public async Task Should_Order_By_Price_Ascending()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("A", 300).Value);
        db.Products.Add(Product.Create("B", 100).Value);
        db.Products.Add(Product.Create("C", 200).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&sort=price");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Select(x => x.Price)
            .Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Should_Fallback_When_Invalid_OrderBy()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("A", 100).Value);
        db.Products.Add(Product.Create("B", 200).Value);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5&sort=invalid");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_Apply_Search_And_Sort_And_Paging()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Products.Add(Product.Create("Laptop A", 300).Value);
        db.Products.Add(Product.Create("Laptop B", 100).Value);
        db.Products.Add(Product.Create("Laptop C", 200).Value);
        db.Products.Add(Product.Create("Phone", 500).Value);
        db.SaveChanges();

        var response = await client.GetAsync(
            $"{TestConstants.ProductsEndpoint}?searchTerm=laptop&sort=price&page=1&pageSize=2");

        var result = await response.Content.ReadFromJsonAsync<
            AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        result!.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Price).Should().BeInAscendingOrder();
    }

    // ---------------- HTTP Client + Cache ----------------

    [Test]
    public async Task Should_Return_Cached_Data_On_Second_Call()
    {
        var factory = new CachedTestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Arrange
        await ProductSeeder.SeedAsync(context);

        // First call → cache is populated
        var res1 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data1 = await res1.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // Change DB AFTER cache is set
        await ProductSeeder.SeedMoreAsync(context);

        // Second call → should return cached data (NOT new DB data)
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data2 = await res2.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // Assert
        data2!.TotalCount.Should().Be(data1!.TotalCount); // 🔥 proves cache hit
    }

    [Test]
    public async Task Should_Invalidate_Cache_After_Create_Product()
    {
        var factory = new CachedTestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await ProductSeeder.SeedAsync(context);

        // First call → cache
        var res1 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data1 = await res1.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // Create new product
        await client.PostAsJsonAsync($"{TestConstants.ProductsEndpoint}", new
        {
            Name = "New Product",
            Price = 100
        });

        // Second call → SHOULD reflect new data (cache invalidated)
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data2 = await res2.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        data2!.TotalCount.Should().BeGreaterThan(data1!.TotalCount);
    }

    [Test]
    public async Task Should_Invalidate_Cache_After_Update_Product()
    {
        var factory = new CachedTestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await ProductSeeder.SeedAsync(context);

        // Get a product to update
        var product = await context.Products.FirstAsync();

        // First call → cache
        var res1 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data1 = await res1.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // Update product
        await client.PutAsJsonAsync($"{TestConstants.ProductsEndpoint}/{product.Id}", new
        {
            Name = "Updated Product",
            Price = 999
        });

        // Second call → should reflect updated data
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data2 = await res2.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // ✅ Assert: updated product appears
        data2!.Items.Should().Contain(x => x.Name == "Updated Product");
    }

    [Test]
    public async Task Should_Invalidate_Cache_After_Delete_Product()
    {
        var factory = new CachedTestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await ProductSeeder.SeedAsync(context);

        // Get a product to delete
        var product = await context.Products.FirstAsync();

        // First call → cache
        var res1 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data1 = await res1.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // Delete product
        await client.DeleteAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");

        // Second call → should reflect deletion
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}?page=1&pageSize=5");
        var data2 = await res2.Content.ReadFromJsonAsync<AppModels.PagedResult<GetListProductsResponse>>(Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        // ✅ Assert: total count decreased
        data2!.TotalCount.Should().Be(data1!.TotalCount - 1);
    }

}
