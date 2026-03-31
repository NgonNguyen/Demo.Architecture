using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Helpers;
using Demo.Architecture.Test.Shared.Json;
using Demo.Architecture.Test.Shared.Web;
using Demo.Architecture.UseCases.Features.Products.Queries.GetById;
using Demo.Architecture.WebAPI.Features.Products.GetById;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Demo.Architecture.Test.WebAPI.IntegrationTests.Products;

[TestFixture]
public class GetProductByIdEndpointTests
{
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
        result.Should().BeOfType<ProblemHttpResult>();
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
    }
}
