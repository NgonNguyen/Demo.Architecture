using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Infrastructure.Serialization;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Helpers;
using Demo.Architecture.Test.Shared.Json;
using Demo.Architecture.Test.Shared.Seeders;
using Demo.Architecture.Test.Shared.Web;
using Demo.Architecture.UseCases.Features.Products.Queries.GetById;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using Demo.Architecture.WebAPI.Features.Products.GetById;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Demo.Architecture.Test.WebAPI.IntegrationTests.Products;

[TestFixture]
public class GetProductByIdEndpointTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new UlidJsonConverter() }
    };

    // ---------------- BASIC (unit) ----------------

    [Test]
    public async Task Should_Return_200_When_Query_Succeeds()
    {
        // Arrange
        var productId = ProductId.New();
        var query = new GetProductByIdQuery(productId);

        var response = new GetProductByIdResponse(
            productId.Value,
            TestConstants.ValidProductNameA,
            TestConstants.ValidPriceA);

        var senderMock = SenderMockHelper.CreateSuccess<GetProductByIdQuery, GetProductByIdResponse>(query, response);

        // Act
        var result = await GetProductByIdEndpoint.Handle(senderMock.Object, query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Ok<GetProductByIdResponse>>();
    }

    [Test]
    public async Task Should_Call_MediatR_With_Correct_Query()
    {
        // Arrange
        var productId = ProductId.New();
        var query = new GetProductByIdQuery(productId);

        var response = new GetProductByIdResponse(productId.Value, TestConstants.ValidProductNameA, TestConstants.ValidPriceA);

        var senderMock = new Mock<ISender>();
        senderMock
            .Setup(x => x.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        // Act
        await GetProductByIdEndpoint.Handle(senderMock.Object, query, CancellationToken.None);

        // Assert
        senderMock.Verify(x => x.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_Return_Problem_When_Result_Fails()
    {
        // Arrange
        var productId = ProductId.New();
        var query = new GetProductByIdQuery(productId);

        var senderMock = SenderMockHelper.CreateFailure<GetProductByIdQuery, GetProductByIdResponse>(
            query,
            new List<ValidationError>
            {
                new() { ErrorMessage = "Something went wrong" }
            });

        // Act
        var result = await GetProductByIdEndpoint.Handle(senderMock.Object, query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    // ---------------- HTTP Client ----------------

    [Test]
    public async Task Should_Return_200_When_Product_Exists()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product = Product.Create(TestConstants.ValidProductNameA, TestConstants.ValidPriceA).Value;
        db.Products.Add(product);
        db.SaveChanges();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id.Value}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetProductByIdResponse>(JsonOptionsHelper.Create());

        result!.Id.Should().Be(product.Id.Value);
        result.Name.Should().Be(TestConstants.ValidProductNameA);
    }

    [Test]
    public async Task Should_Return_404_When_Product_Not_Exists()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var id = Ulid.NewUlid();

        var response = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptionsHelper.Create());

        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Not Found");
        problem.Status.Should().Be(StatusCodes.Status404NotFound);
        problem.Detail.Should().Be("Product not found");
        var errorCodeObj = problem.Extensions["errorCode"];
        var errorCode = errorCodeObj is JsonElement je ? je.GetString() : errorCodeObj?.ToString();
        errorCode.Should().Be(ProductErrors.NotFound.Code);
        problem.Instance.Should().Be($"/api/products/{id}");
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
        await ProductSeeder.SeedAsync(context);

        var product = await context.Products.FirstAsync();

        // First call → hits DB
        var res1 = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");
        var data1 = await res1.Content.ReadFromJsonAsync<GetProductByIdResponse>(_options);

        // Second call → should hit cache
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");
        var data2 = await res2.Content.ReadFromJsonAsync<GetProductByIdResponse>(_options);

        data2!.Id.Should().Be(data1!.Id);
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

        var product = await context.Products.FirstAsync();

        // First call → cache
        var res1 = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");
        var data1 = await res1.Content.ReadFromJsonAsync<GetProductByIdResponse>(_options);

        // Update product
        await client.PutAsJsonAsync($"{TestConstants.ProductsEndpoint}/{product.Id}", new
        {
            Name = "Updated Product",
            Price = 999
        });

        // Second call → should reflect updated data
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");
        var data2 = await res2.Content.ReadFromJsonAsync<GetProductByIdResponse>(_options);

        data2!.Name.Should().Be("Updated Product");
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

        var product = await context.Products.FirstAsync();

        // First call → cache
        await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");

        // Delete product
        await client.DeleteAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");

        // Second call → should be NotFound (cache invalidated)
        var res2 = await client.GetAsync($"{TestConstants.ProductsEndpoint}/{product.Id}");

        res2.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
