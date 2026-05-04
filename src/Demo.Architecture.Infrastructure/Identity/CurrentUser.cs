using Demo.Architecture.UseCases.Common.Identity;
using Microsoft.AspNetCore.Http;

namespace Demo.Architecture.Infrastructure.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserInfo GetUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return new UserInfo(
            user?.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException(),
            user?.FindFirst("email")?.Value,
            user?.FindAll("role").Select(r => r.Value).ToArray() ?? []
        );
    }
}
