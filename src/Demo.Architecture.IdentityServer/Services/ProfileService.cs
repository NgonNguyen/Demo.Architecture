using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Demo.Architecture.IdentityServer.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim("email", user.Email!)
        };

        claims.AddRange(roles.Select(r => new Claim("role", r)));

        // 🔥 Permissions mapping
        var permissions = new List<string>();

        if (roles.Contains("CompanyAdmin"))
        {
            permissions.Add("Product.Read");
            permissions.Add("Product.Write");
        }

        if (roles.Contains("Staff"))
        {
            permissions.Add("Product.Read");
        }

        // Add permission claims
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        context.IssuedClaims.AddRange(
            claims.Where(c => context.RequestedClaimTypes.Contains(c.Type)));
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
    }
}
