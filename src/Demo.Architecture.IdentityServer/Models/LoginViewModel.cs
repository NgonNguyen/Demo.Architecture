namespace Demo.Architecture.IdentityServer.Models;

public class LoginViewModel
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string? ReturnUrl { get; set; }
}
