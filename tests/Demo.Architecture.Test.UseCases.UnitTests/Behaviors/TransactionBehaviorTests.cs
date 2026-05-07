using Ardalis.Result;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Common.Messaging.Commands;
using FluentAssertions;
using MediatR;
using Moq;
using NUlid;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Behaviors;

public class TransactionBehaviorTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private TransactionBehavior<FakeCommand, Result<Ulid>> _commandBehavior = null!;
    private TransactionBehavior<FakeQuery, Result<Ulid>> _queryBehavior = null!;

    private FakeCommand _command = null!;
    private FakeQuery _query = null!;

    private Result<Ulid> _successResponse = null!;
    private Result<Ulid> _failureResponse = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();

        _commandBehavior = new TransactionBehavior<FakeCommand, Result<Ulid>>(_contextMock.Object);
        _queryBehavior = new TransactionBehavior<FakeQuery, Result<Ulid>>(_contextMock.Object);

        _command = new FakeCommand();
        _query = new FakeQuery();

        _successResponse = Result<Ulid>.Success(Ulid.NewUlid());
        _failureResponse = Result<Ulid>.Error("error");
    }

    [Test]
    public async Task Should_Call_SaveChanges_When_Command_Succeeds()
    {
        // Arrange
        RequestHandlerDelegate<Result<Ulid>> next = (ct) => Task.FromResult(_successResponse);

        // Act
        await _commandBehavior.Handle(_command, next, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_Not_Call_SaveChanges_When_Command_Fails()
    {
        // Arrange
        RequestHandlerDelegate<Result<Ulid>> next = (ct) => Task.FromResult(_failureResponse);

        // Act
        await _commandBehavior.Handle(_command, next, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_Not_Call_SaveChanges_For_Query()
    {
        // Arrange
        RequestHandlerDelegate<Result<Ulid>> next = (ct) => Task.FromResult(_successResponse);

        // Act
        await _queryBehavior.Handle(_query, next, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_Always_Call_Next()
    {
        // Arrange
        var called = false;
        RequestHandlerDelegate<Result<Ulid>> next = (ct) =>
        {
            called = true;
            return Task.FromResult(_successResponse);
        };

        // Act
        await _commandBehavior.Handle(_command, next, CancellationToken.None);

        // Assert
        called.Should().BeTrue();
    }
}

public record FakeCommand() : ICommand<Result<Ulid>>;

public record FakeQuery() : IRequest<Result<int>>;