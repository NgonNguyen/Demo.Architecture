using Demo.Architecture.UseCases.Common.Identity;
using MediatR;

namespace Demo.Architecture.UseCases.Common.Behaviors;

public class InjectUserInfoBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICurrentUser _currentUser;

    public InjectUserInfoBehavior(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IHasUserInfo hasUserInfo)
        {
            hasUserInfo.UserInfo = _currentUser.GetUser(); // ⚠️ see note below
        }

        return await next();
    }
}
