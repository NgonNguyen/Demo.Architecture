using Ardalis.Result;
using MediatR;
using Moq;

namespace Demo.Architecture.Test.Shared.Helpers;

public static class SenderMockHelper
{
    public static Mock<ISender> CreateSuccess<TQuery, TResponse>(
        TQuery query,
        TResponse response)
        where TQuery : IRequest<Result<TResponse>>
    {
        var mock = new Mock<ISender>();

        mock.Setup(x => x.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        return mock;
    }

    public static Mock<ISender> CreateFailure<TQuery, TResponse>(
        TQuery query,
        List<ValidationError> errors)
        where TQuery : IRequest<Result<TResponse>>
    {
        var mock = new Mock<ISender>();

        mock.Setup(x => x.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Invalid(errors));

        return mock;
    }
}
