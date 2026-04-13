using Ardalis.Result;
using Demo.Architecture.UseCases.Common.Attributes;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Idempotency;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUlid;
using NUnit.Framework;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Demo.Architecture.Test.UseCases.UnitTests.Behaviors;

public class IdempotencyBehaviorTests
{
    private readonly Mock<IIdempotencyService> _service = new();
    private readonly Mock<IHttpContextAccessor> _http = new();

    private readonly IdempotencyOptions _options = new()
    {
        CacheTtl = TimeSpan.FromMinutes(10),
        LockTtl = TimeSpan.FromSeconds(30)
    };

    private IdempotencyBehavior<TestRequest, Result<Ulid>> CreateBehavior()
    {
        return new IdempotencyBehavior<TestRequest, Result<Ulid>>(
            _service.Object,
            _http.Object,
            Options.Create(_options));
    }

    private static DefaultHttpContext CreateHttpContext(string? key = null)
    {
        var context = new DefaultHttpContext();

        if (key != null)
            context.Request.Headers["Idempotency-Key"] = key;

        context.Request.Path = "/test";

        return context;
    }

    [Test]
    public async Task Should_Call_Next_When_Key_Not_Provided_And_Not_Required()
    {
        var behavior = CreateBehavior();

        _http.Setup(x => x.HttpContext)
            .Returns(CreateHttpContext(null));

        var nextCalled = false;

        var result = await behavior.Handle(
            new TestRequest("A"),
            ct =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success(Ulid.NewUlid()));
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);
    }

    [Test]
    public async Task Should_Return_Invalid_When_Key_Missing_And_Required()
    {
        var behavior = new IdempotencyBehavior<RequiredRequest, Result<Ulid>>(
            _service.Object,
            _http.Object,
            Options.Create(_options));

        _http.Setup(x => x.HttpContext)
            .Returns(CreateHttpContext(null));

        var result = await behavior.Handle(
            new RequiredRequest("A"),
            ct => throw new Exception("Should not be called"),
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Test]
    public async Task Should_Return_Cached_Response_When_Same_Request_Replayed()
    {
        var behavior = CreateBehavior();

        var request = new TestRequest("A");

        var context = CreateHttpContext("key-1");
        _http.Setup(x => x.HttpContext).Returns(context);

        var response = Result.Success(Ulid.NewUlid());

        var json = JsonSerializer.Serialize(
            response,
            Architecture.Shared.Serialization.JsonSerializerDefaults.Options);

        var hash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(request, Architecture.Shared.Serialization.JsonSerializerDefaults.Options)
            )));

        _service.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(new IdempotencyRecord
            {
                RequestHash = hash,
                ResponseJson = json
            });

        var result = await behavior.Handle(
            request,
            ct => throw new Exception("Should not be called"),
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Ok);
    }

    [Test]
    public async Task Should_Return_Invalid_When_Same_Key_Different_Request()
    {
        var behavior = CreateBehavior();

        _http.Setup(x => x.HttpContext)
            .Returns(CreateHttpContext("key-1"));

        _service.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(new IdempotencyRecord
            {
                RequestHash = "different-hash"
            });

        var result = await behavior.Handle(
            new TestRequest("A"),
            ct => throw new Exception("Should not be called"),
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Test]
    public async Task Should_Return_Invalid_When_Lock_Not_Acquired()
    {
        var behavior = CreateBehavior();

        _http.Setup(x => x.HttpContext)
            .Returns(CreateHttpContext("key-1"));

        _service.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        _service.Setup(x => x.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((false, string.Empty));

        var result = await behavior.Handle(
            new TestRequest("A"),
            ct => throw new Exception("Should not be called"),
            CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Test]
    public async Task Should_Execute_And_Save_When_Lock_Acquired()
    {
        var behavior = CreateBehavior();
        var token = "test-token"; // ✅ avoid magic string duplication

        _http.Setup(x => x.HttpContext)
            .Returns(CreateHttpContext("key-1"));

        _service.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        _service.Setup(x => x.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((true, token));

        var saved = false;

        _service.Setup(x => x.SaveAsync(
                It.IsAny<string>(),
                It.IsAny<IdempotencyRecord>(),
                It.IsAny<TimeSpan>()))
            .Callback(() => saved = true)
            .Returns(Task.CompletedTask);

        var result = await behavior.Handle(
            new TestRequest("A"),
            ct => Task.FromResult(Result.Success(Ulid.NewUlid())),
            CancellationToken.None);

        saved.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);
    }

    [Test]
    public async Task Should_Always_Release_Lock()
    {
        var behavior = CreateBehavior();

        const string token = "test-token";

        _http.Setup(x => x.HttpContext)
            .Returns(CreateHttpContext("key-1"));

        _service.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        _service.Setup(x => x.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((true, token)); // ✅ updated

        _service.Setup(x => x.ReleaseLockAsync(It.IsAny<string>(), token))
            .ReturnsAsync(true); // ✅ updated

        await behavior.Handle(
            new TestRequest("A"),
            ct => Task.FromResult(Result.Success(Ulid.NewUlid())),
            CancellationToken.None);

        // ✅ verify release ALWAYS happens
        _service.Verify(
            x => x.ReleaseLockAsync(It.IsAny<string>(), token),
            Times.Once);
    }
}

public record TestRequest(string Name) : IRequest<Result<Ulid>>;

[RequireIdempotency]
public record RequiredRequest(string Name) : IRequest<Result<Ulid>>;