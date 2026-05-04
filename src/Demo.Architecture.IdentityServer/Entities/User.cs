using NUlid;

namespace Demo.Architecture.IdentityServer.Entities;

public class User
{
    public Ulid UserId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}

