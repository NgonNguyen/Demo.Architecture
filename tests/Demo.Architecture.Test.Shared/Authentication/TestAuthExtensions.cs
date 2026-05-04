using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Architecture.Test.Shared.Authentication;

public static class TestAuthExtensions
{
    public static IServiceCollection AddTestAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(TestAuthDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthDefaults.AuthenticationScheme,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                    TestAuthDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = TestAuthDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = TestAuthDefaults.AuthenticationScheme;
        });

        return services;
    }
}
