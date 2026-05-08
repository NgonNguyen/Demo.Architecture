namespace Demo.Architecture.WebAPI.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ProductScope", policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == "scope" &&
                        c.Value.Split(' ').Contains("product-api")));
            });

            options.AddPolicy("AdminOnly",
                policy => policy.RequireRole("Admin"));

            options.AddPolicy("CompanyAdminOnly",
                policy => policy.RequireRole("CompanyAdmin"));

            options.AddPolicy("StaffOnly",
                policy => policy.RequireRole("Staff"));

            options.AddPolicy("ProductRead",
                policy => policy.RequireClaim("permission", "Product.Read"));

            options.AddPolicy("ProductWrite",
                policy => policy.RequireClaim("permission", "Product.Write"));
        });

        return services;
    }
}
