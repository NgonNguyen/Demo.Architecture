using Microsoft.IdentityModel.Tokens;

namespace Demo.Architecture.WebAPI.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "https://localhost:7182";

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = "product-api",
                        RoleClaimType = "role",
                        NameClaimType = "email"
                    };
            });

        return services;
    }
}
