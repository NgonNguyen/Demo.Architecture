using Ardalis.Result;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Helpers;
using Demo.Architecture.Test.Shared.Json;
using Demo.Architecture.Test.Shared.Web;
using Demo.Architecture.UseCases.Features.Products.Commands.Create;
using Demo.Architecture.UseCases.Features.Products.Commands.Update;
using Demo.Architecture.WebAPI.Features.Products.Update;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Demo.Architecture.Test.WebAPI.IntegrationTests.Products;

public class UpdateProductEndpointTests
{
    // ---------------- BASIC (unit) ----------------

    [Test]
    public async Task Should_Return_204_When_Update_Succeeds()
    {
        var id = Ulid.NewUlid();

        var request = new UpdateProductRequest(
            TestConstants.ValidProductNameA,
            TestConstants.ValidPriceA);

        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Price);

        var senderMock = SenderMockHelper.CreateCommandSuccess(command);

        var result = await UpdateProductEndpoint.Handle(
            senderMock.Object,
            id,
            request,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<NoContent>();
    }

    [Test]
    public async Task Should_Call_MediatR_With_Correct_Command()
    {
        var id = Ulid.NewUlid();

        var request = new UpdateProductRequest(
            TestConstants.ValidProductNameA,
            TestConstants.ValidPriceA);

        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Price);

        var senderMock = new Mock<ISender>();

        senderMock
            .Setup(x => x.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        await UpdateProductEndpoint.Handle(
            senderMock.Object,
            id,
            request,     
            CancellationToken.None);

        // 🔥 Important: verify Id is overridden from route
        senderMock.Verify(x =>
            x.Send(It.Is<UpdateProductCommand>(c =>
                c.Id == id &&
                c.Name == command.Name &&
                c.Price == command.Price),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Should_Return_Problem_When_Result_Fails()
    {
        var id = Ulid.NewUlid();

        var request = new UpdateProductRequest(string.Empty, 0);

        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Price);

        var senderMock = SenderMockHelper.CreateCommandFailure<UpdateProductCommand>(
            command,
            new List<ValidationError>
            {
                new() { ErrorMessage = "Something went wrong" }
            });

        var result = await UpdateProductEndpoint.Handle(senderMock.Object, id, request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    [Test]
    public async Task Should_Return_Problem_When_Validation_Fails()
    {
        var id = Ulid.NewUlid();

        var request = new UpdateProductRequest(string.Empty, 0);

        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Price);

        var senderMock = SenderMockHelper.CreateCommandFailure(
            command,
            new List<ValidationError>
            {
                new() { ErrorMessage = "Invalid" }
            });

        var result = await UpdateProductEndpoint.Handle(
            senderMock.Object,
            id,
            request,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    [Test]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var id = Ulid.NewUlid();

        var request = new UpdateProductRequest(
            TestConstants.ValidProductNameA,
            TestConstants.ValidPriceA);

        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Price);

        var senderMock = SenderMockHelper.CreateCommandNotFound(command);

        var result = await UpdateProductEndpoint.Handle(
            senderMock.Object,
            id,
            request,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    // ---------------- HTTP Client ----------------

    [Test]
    public async Task Should_Return_204_When_Valid()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange - create product first
        var createResponse = await client.PostAsJsonAsync(
            TestConstants.ProductsEndpoint,
            new CreateProductCommand(
                TestConstants.ValidProductNameA,
                TestConstants.ValidPriceA));

        var createdId = await createResponse.Content
            .ReadFromJsonAsync<Ulid>(JsonOptionsHelper.Create());

        // Act
        var response = await client.PutAsJsonAsync(
            $"{TestConstants.ProductsEndpoint}/{createdId}",
            new
            {
                name = TestConstants.ValidProductNameB,
                price = TestConstants.ValidPriceB
            });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Should_Return_400_When_Invalid()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange - create product first
        var createResponse = await client.PostAsJsonAsync(
            TestConstants.ProductsEndpoint,
            new CreateProductCommand(
                TestConstants.ValidProductNameA,
                TestConstants.ValidPriceA));

        var createdId = await createResponse.Content
            .ReadFromJsonAsync<Ulid>(JsonOptionsHelper.Create());

        var response = await client.PutAsJsonAsync(
            $"{TestConstants.ProductsEndpoint}/{createdId}",
            new
            {
                name = "",
                price = 0
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>(JsonOptionsHelper.Create());

        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Validation Error");
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Extensions.Should().ContainKey("errors");

        var errorsJson = problem.Extensions["errors"];

        var errors = JsonSerializer.Deserialize<List<ValidationErrorDto>>(
            JsonSerializer.Serialize(errorsJson),
            JsonOptionsHelper.Create());

        errors.Should().NotBeNull();

        errors.Should().Contain(e =>
            e.Field == nameof(Product.Name) &&
            e.Code == ProductErrors.NameRequired.Code &&
            e.Message == ProductErrors.NameRequired.Message);

        errors.Should().Contain(e =>
            e.Field == nameof(Product.Price) &&
            e.Code == ProductErrors.PriceInvalid.Code &&
            e.Message == ProductErrors.PriceInvalid.Message);
    }

    [Test]
    public async Task Should_Return_400_When_Name_Already_Exists()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange - create two products
        var createResponseA = await client.PostAsJsonAsync(
            TestConstants.ProductsEndpoint,
            new CreateProductCommand("ExistingNameA", 100));

        var idA = await createResponseA.Content
            .ReadFromJsonAsync<Ulid>(JsonOptionsHelper.Create());

        var createResponseB = await client.PostAsJsonAsync(
            TestConstants.ProductsEndpoint,
            new CreateProductCommand("ExistingNameB", 200));

        var idB = await createResponseB.Content
            .ReadFromJsonAsync<Ulid>(JsonOptionsHelper.Create());

        // Act - try to update product B to have product A’s name
        var response = await client.PutAsJsonAsync(
            $"{TestConstants.ProductsEndpoint}/{idB}",
            new
            {
                name = "ExistingNameA",
                price = 300
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>(JsonOptionsHelper.Create());

        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Validation Error");
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Extensions.Should().ContainKey("errors");

        var errorsJson = problem.Extensions["errors"];

        var errors = JsonSerializer.Deserialize<List<ValidationErrorDto>>(
            JsonSerializer.Serialize(errorsJson),
            JsonOptionsHelper.Create());

        errors.Should().NotBeNull();

        errors.Should().Contain(e =>
            e.Field == nameof(Product.Name) &&
            e.Message == ProductErrors.DuplicatedName.Message &&
            e.Code == ProductErrors.DuplicatedName.Code);
    }

    [Test]
    public async Task Should_Return_404_When_NotFound()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var id = Ulid.NewUlid();

        var response = await client.PutAsJsonAsync(
            $"{TestConstants.ProductsEndpoint}/{id}",
            new
            {
                name = TestConstants.ValidProductNameA,
                price = TestConstants.ValidPriceA
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
