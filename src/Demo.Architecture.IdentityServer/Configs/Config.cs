
using Duende.IdentityServer.Models;

namespace Demo.Architecture.IdentityServer.Configs;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("product-api", "Product API")
            {
                Scopes = { "product-api" },
                UserClaims = { "role", "email", "permission" } // 👈 include claims in token
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("product-api", "Product API")
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "postman",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris =
                {
                    "https://oauth.pstmn.io/v1/callback"
                },

                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "product-api",
                    "offline_access"
                },

                AllowOfflineAccess = true, // 👈 REQUIRED for refresh token
                AccessTokenLifetime = 900, // 15 min
                RefreshTokenUsage = TokenUsage.OneTimeOnly, // rotation
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 3600, // 1 hour
                AbsoluteRefreshTokenLifetime = 86400 // 1 day
            },
            new Client
            {
                ClientId = "web-client",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = { "https://localhost:3000/callback" },

                AllowedScopes = { "openid", "profile", "product-api" },

                AllowOfflineAccess = true // refresh token
            }
        };

    //public static IEnumerable<ApiScope> ApiScopes =>
    //    new List<ApiScope>
    //    {
    //        new ApiScope("product-api", "Product API")
    //    };

    //public static IEnumerable<Client> Clients =>
    //    new List<Client>
    //    {
    //        new Client
    //        {
    //            ClientId = "postman-client",
    //            AllowedGrantTypes = GrantTypes.ClientCredentials,
    //            ClientSecrets =
    //            {
    //                new Secret("secret".Sha256())
    //            },

    //            AllowedScopes = { "product-api" }
    //        }
    //    };
}
