using Demo.Architecture.UseCases.Common.Identity;

namespace Demo.Architecture.Test.Shared.Identity;

public class FakeCurrentUser : ICurrentUser
{
    public UserInfo GetUser()
    {
        return new UserInfo(
            "test-user-id",
            "test@email.com",
            new[] { "Admin" }
        );
    }
}
