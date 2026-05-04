using Demo.Architecture.IdentityServer.Configs;
using Demo.Architecture.IdentityServer.Contexts;
using Demo.Architecture.IdentityServer.Entities;
using Demo.Architecture.IdentityServer.Services;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NUlid;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5077); // HTTP
    options.ListenLocalhost(7182, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "app.db");

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddControllersWithViews();

var dbAuthPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "auth.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbAuthPath}"));



builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
});

builder.Services.AddIdentityServer()
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryClients(Config.Clients)
    .AddAspNetIdentity<IdentityUser>()
    .AddDeveloperSigningCredential();

builder.Services.AddScoped<IProfileService, ProfileService>();

//.AddInMemoryIdentityResources(new List<IdentityResource> // ✅ VERY IMPORTANT
//{
//    new IdentityResources.OpenId(),
//    new IdentityResources.Profile()
//})
//.AddInMemoryApiScopes(new List<ApiScope>
//{
//    new ApiScope("product-api", "Product API")
//})
//.AddInMemoryClients(new List<Client>
//{
//    new Client
//    {
//        ClientId = "web-client",

//        AllowedGrantTypes = GrantTypes.Code,
//        RequirePkce = true,
//        RequireClientSecret = false,

//        RedirectUris = { "https://localhost:3000/callback" },

//        AllowedScopes = { "openid", "profile", "product-api" },

//        AllowOfflineAccess = true
//    }
//})
//.AddAspNetIdentity<IdentityUser>()
//.AddDeveloperSigningCredential();

//builder.Services.AddIdentityServer()
//    .AddInMemoryApiScopes(Config.ApiScopes)
//    .AddInMemoryClients(Config.Clients)
//    .AddAspNetIdentity<IdentityUser>()
//    .AddDeveloperSigningCredential();


//builder.Services.AddDbContext<Demo.Architecture.IdentityServer.Contexts.MyDbContext>(options =>
//    options.UseSqlite($"Data Source={dbPath}"));

//builder.Services.AddIdentityServer()
//    .AddInMemoryClients(Config.Clients)
//    .AddInMemoryApiScopes(Config.ApiScopes)
//    .AddDeveloperSigningCredential(); // dev only

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

// 🔑 Seed users here
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Create role
    var roles = new[] { "Admin", "CompanyAdmin", "Staff" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    //if (!await roleManager.RoleExistsAsync("Admin"))
    //{
    //    await roleManager.CreateAsync(new IdentityRole("Admin"));
    //}

    var user = await userManager.FindByEmailAsync("test@test.com");

    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }

    // CompanyAdmin user
    var admin = await userManager.FindByEmailAsync("admin@company.com");
    if (admin == null)
    {
        admin = new IdentityUser
        {
            UserName = "admin@company.com",
            Email = "admin@company.com",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, "Password123!");
        await userManager.AddToRoleAsync(admin, "CompanyAdmin");
    }

    // Staff user
    var staff = await userManager.FindByEmailAsync("staff@company.com");
    if (staff == null)
    {
        staff = new IdentityUser
        {
            UserName = "staff@company.com",
            Email = "staff@company.com",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(staff, "Password123!");
        await userManager.AddToRoleAsync(staff, "Staff");
    }
}

//using (var scope = app.Services.CreateScope())
//{
//    var userManager = scope.ServiceProvider
//        .GetRequiredService<UserManager<IdentityUser>>();

//    var user = await userManager.FindByEmailAsync("test@test.com");
//    if (user == null)
//    {
//        user = new IdentityUser { UserName = "test@test.com", Email = "test@test.com" };
//        await userManager.CreateAsync(user, "Password123!");
//    }
//}

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
//    db.Database.EnsureCreated(); // make sure DB exists

//    if (!db.Users.Any())
//    {
//        var hasher = new PasswordHasher<User>();

//        var alice = new User
//        {
//            UserId = Ulid.NewUlid(),
//            Email = "alice@example.com",
//            PasswordHash = hasher.HashPassword(null, "alice123")
//        };

//        var bob = new User
//        {
//            UserId = Ulid.NewUlid(),
//            Email = "bob@example.com",
//            PasswordHash = hasher.HashPassword(null, "bob456")
//        };

//        db.Users.AddRange(alice, bob);
//        db.SaveChanges();
//    }
//}

// Simple login endpoint for testing (returns user info without JWT, just to verify password hashing and user retrieval works)



// Login endpoint for testing (not needed if using client credentials flow, but useful for demoing JWT generation with user claims)
// It has aud: webapi and scope: product-api claims, which matches the IdentityServer config. You can test this with Postman or curl to get a JWT for Alice or Bob, then use that token to call your protected API endpoints in Demo.Architecture.WebAPI.
/*
app.MapPost("/login", async (LoginRequest request, MyDbContext db) =>
{
    // Find user by email
    var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
    if (user == null) return Results.Unauthorized();

    // Verify password using ASP.NET Core's PasswordHasher
    var hasher = new PasswordHasher<User>();
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

    if (result == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    // Build claims for JWT
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),   // ULID stored as string
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("scope", "product-api")                      // optional: add scope claim
    };

    // Load signing key (better to keep in appsettings.json)
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes("this_is_a_very_long_super_secret_key_1234567890")
    );
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "https://localhost:7182",
        audience: "webapi",
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    return Results.Ok(new
    {
        access_token = new JwtSecurityTokenHandler().WriteToken(token),
        token_type = "Bearer",
        expires_in = 3600
    });
});
*/

app.UseStaticFiles();
app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
