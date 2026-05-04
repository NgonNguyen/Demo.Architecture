using Demo.Architecture.UseCases.Common.Identity;
using Mediator;

namespace Demo.Architecture.UseCases.Common.Messaging.Commands;

public abstract record BaseCommand<TResponse>
    : ICommand<TResponse>, IHasUserInfo
{
    public UserInfo UserInfo { get; set; } = default!;
}
