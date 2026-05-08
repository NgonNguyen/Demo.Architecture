using Demo.Architecture.WebAPI.Extensions;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.Console()
    .CreateLogger();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Services
builder.Services
    .AddExceptionHandling()
    .AddOpenTelemetryServices(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddAuthorizationPolicies()
    .AddApplicationServices()
    .AddMediatRServices()
    .AddPipelineBehaviors(builder)
    .AddRedisCache(builder.Configuration)
    .AddDatabase(builder)
    .AddMassTransitServices(builder)
    .AddJsonConfiguration()
    .AddOpenApiDocumentation();

var app = builder.Build();

// Middleware
app.UseApplicationMiddlewares();

app.Run();