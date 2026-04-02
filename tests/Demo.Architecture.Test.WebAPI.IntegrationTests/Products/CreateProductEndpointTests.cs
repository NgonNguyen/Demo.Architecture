using Ardalis.Result;
using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.Test.Shared.Helpers;
using Demo.Architecture.Test.Shared.Json;
using Demo.Architecture.Test.Shared.Web;
using Demo.Architecture.UseCases.Features.Products.Commands.Create;
using Demo.Architecture.WebAPI.Features.Products.Create;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Demo.Architecture.Test.WebAPI.IntegrationTests.Products;

[TestFixture]
public class CreateProductEndpointTests
{
    // ---------------- BASIC (unit) ----------------

    [Test]
    public async Task Should_Return_201_When_Create_Succeeds()
    {
        var command = new CreateProductCommand(TestConstants.ValidProductNameA, TestConstants.ValidPriceA);

        var ulid = Ulid.NewUlid();

        var senderMock = SenderMockHelper.CreateSuccess<CreateProductCommand, Ulid>(command, ulid);

        var result = await CreateProductEndpoint.Handle(senderMock.Object, command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<Ok<Ulid>>();
    }

    [Test]
    public async Task Should_Call_MediatR_With_Correct_Command()
    {
        var command = new CreateProductCommand(TestConstants.ValidProductNameA, TestConstants.ValidPriceA);

        var ulid = Ulid.NewUlid();

        var senderMock = new Mock<ISender>();
        senderMock
            .Setup(x => x.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ulid));

        await CreateProductEndpoint.Handle(senderMock.Object, command, CancellationToken.None);

        senderMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_Return_Problem_When_Validation_Fails()
    {

        var command = new CreateProductCommand(string.Empty, 0);

        var senderMock = SenderMockHelper.CreateFailure<CreateProductCommand, Ulid>(
            command,
            new List<ValidationError>
            {
                new() { ErrorMessage = "Invalid" }
            });

        var result = await CreateProductEndpoint.Handle(
            senderMock.Object,
            command,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    [Test]
    public async Task Should_Return_Problem_When_Result_Fails()
    {
        var command = new CreateProductCommand(string.Empty, 0);

        var senderMock = SenderMockHelper.CreateFailure<CreateProductCommand, Ulid>(
            command,
            new List<ValidationError>
            {
                new() { ErrorMessage = "Something went wrong" }
            });

        var result = await CreateProductEndpoint.Handle(senderMock.Object, command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<Architecture.WebAPI.Common.Results.ProblemHttpResult>();
    }

    // ---------------- HTTP Client ----------------

    [Test]
    public async Task Should_Return_201_When_Valid()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var command = new CreateProductCommand(TestConstants.ValidProductNameA, TestConstants.ValidPriceA);

        var response = await client.PostAsJsonAsync(TestConstants.ProductsEndpoint, command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Ulid>(JsonOptionsHelper.Create());

        result.Should().NotBe(default(Ulid));
    }

    [Test]
    public async Task Should_Return_400_When_Invalid()
    {
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var command = new CreateProductCommand(string.Empty, 0);

        var response = await client.PostAsJsonAsync(TestConstants.ProductsEndpoint, command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(JsonOptionsHelper.Create());

        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Validation Error");
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Extensions.Should().ContainKey("errors");

        // 👇 Extract errors
        var errorsJson = problem.Extensions["errors"];

        var errors = JsonSerializer.Deserialize<List<ValidationErrorDto>>(
            JsonSerializer.Serialize(errorsJson),
            JsonOptionsHelper.Create());

        errors.Should().NotBeNull();
        errors!.Should().HaveCount(2);

        errors.Should().Contain(e =>
            e.Field == "name" &&
            e.Message == "Name is required" &&
            e.Code == "PRODUCT_NAME_REQUIRED");

        errors.Should().Contain(e =>
            e.Field == "price" &&
            e.Message == "Price must be > 0" &&
            e.Code == "PRODUCT_PRICE_INVALID");
    }
}

public class ValidationErrorDto
{
    public string Field { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Code { get; set; } = default!;
}