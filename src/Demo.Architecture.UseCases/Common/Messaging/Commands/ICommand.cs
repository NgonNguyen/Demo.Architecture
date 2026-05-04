using MediatR;

namespace Demo.Architecture.UseCases.Common.Messaging.Commands;

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
