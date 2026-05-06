using Demo.Architecture.Infrastructure;
using Demo.Architecture.Infrastructure.Caching;
using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Infrastructure.Features.Products;
using Demo.Architecture.Infrastructure.Identity;
using Demo.Architecture.Infrastructure.Messaging.Extensions;
using Demo.Architecture.Infrastructure.Messaging.RabbitMQ;
using Demo.Architecture.Infrastructure.Observability;
using Demo.Architecture.Shared.Serialization;
using Demo.Architecture.UseCases;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Identity;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Rules;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Configurations;
using Demo.Architecture.WebAPI.Middlewares;
using Demo.Architecture.WebAPI.OpenApi.Processors;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

// This is abc the entry point of the application, where we configure services, middleware, and the HTTP request pipeline.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.Console()
    .CreateLogger();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "web-api"))
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()   // captures request metrics
            .AddHttpClientInstrumentation()   // captures outgoing HTTP calls
            .AddRuntimeInstrumentation()      // captures .NET runtime stats
            .AddPrometheusExporter();         // exposes /metrics endpoint
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddSource("Demo.Architecture")
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddEntityFrameworkCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:4317");
            })
            .AddConsoleExporter();
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            context.HttpContext.Request.Path;
    };
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7182";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = "product-api",
            RoleClaimType = "role",      // 🔥 THIS LINE FIXES IT
            NameClaimType = "email"      // (optional, useful)
        };

        /*options.Authority = "https://localhost:7182"; // AuthServer

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false // ✅ important for now
        };*/

        // For basic AuthenServer with token (without username & password) only use token from /connect/token
        // options.Authority = "https://localhost:7182"; // AuthServer
        // options.TokenValidationParameters.ValidateAudience = false;

        // For advanced AuthServer with token (login with username & password)
        /*options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://localhost:7182",   // must match issuer in JwtSecurityToken

            ValidateAudience = true,
            ValidAudience = "webapi",                 // must match audience in JwtSecurityToken

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("this_is_a_very_long_super_secret_key_1234567890")
            )
        };*/
    });

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("ProductScope", policy =>
    //{
    //    policy.RequireClaim("scope", "product-api");
    //});

    options.AddPolicy("ProductScope", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "scope" &&
                c.Value.Split(' ').Contains("product-api")
            ));
    });

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });

    options.AddPolicy("CompanyAdminOnly", policy =>
        policy.RequireRole("CompanyAdmin"));

    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("Staff"));

    options.AddPolicy("ProductRead", policy =>
    {
        policy.RequireClaim("permission", "Product.Read");
    });

    options.AddPolicy("ProductWrite", policy =>
    {
        policy.RequireClaim("permission", "Product.Write");
    });

    //options.AddPolicy("CompanyOrStaff", policy =>
    //{
    //    policy.RequireRole("CompanyAdmin", "Staff");
    //});

    /*
    options.AddPolicy("ProductScope", policy =>
    {
        policy.RequireClaim("scope", "product-api");
    });*/
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

builder.Services.AddAuthorization();
// -----------------------------
// MediatR
// -----------------------------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration!);
});

builder.Services.AddScoped<IProductUniquenessChecker, ProductUniquenessChecker>();

builder.Services.AddValidatorsFromAssembly(typeof(IProductUniquenessChecker).Assembly);

builder.Services.AddScoped<ICacheService, RedisCacheService>();

if (!builder.Environment.IsEnvironment("Test"))
{
    // use RabbitMQ.Client directly for the publisher, without MassTransit, to demonstrate the difference between using a library vs. direct implementation
    /*
    builder.Services.AddSingleton<IConnection>(sp =>
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    });

    builder.Services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

    */

    // MassTransit with RabbitMQ
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ApplyRoutingKeyAttributes(
                typeof(IntegrationEventsAssemblyMarker).Assembly
            );

            cfg.UseRawJsonSerializer();

            cfg.ConfigureJsonSerializerOptions(options =>
            {
                options.Converters.Add(new UlidJsonConverter());
                return options;
            });
        });
    });

    builder.Services.AddIdempotency();  
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InjectUserInfoBehavior<,>));
}

// -----------------------------
// Database (SQLite)
// -----------------------------

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IReadOnlyApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "app.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// -----------------------------
// Controllers + JSON (ULID)
// -----------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new UlidJsonConverter());
    });

// -----------------------------
// NSwag (OpenAPI)
// -----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Demo Architecture API";

    config.UseControllerSummaryAsTagDescription = true;
    config.OperationProcessors.Add(new InternalServerErrorResponseOperationProcessor());
    config.OperationProcessors.Add(new ResponseExampleOperationProcessor());
    config.OperationProcessors.Add(new RequestExampleOperationProcessor());
    config.OperationProcessors.Add(new NotFoundResponseOperationProcessor());
    config.OperationProcessors.Add(new BadRequestResponseOperationProcessor());
    config.OperationProcessors.Add(new QueryExampleOperationProcessor());
    config.OperationProcessors.Add(new IdempotencyHeaderOperationProcessor());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new UlidJsonConverter());
});

builder.Services.AddOpenTelemetry();

// -----------------------------
var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpMetrics();
app.MapMetrics();

// -----------------------------
// Middleware
// -----------------------------
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserLoggingMiddleware>();

// -----------------------------
// NSwag UI
// -----------------------------
//app.UseOpenApi(); // /swagger/v1/swagger.json
//// app.UseOpenApi();
//app.UseReDoc();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseOpenApi();
    app.UseReDoc(c =>
    {
        c.Path = "/docs";
        c.DocumentPath = "/swagger/v1/swagger.json";
    });
}

app.UseSwaggerUi(settings =>
{
    settings.Path = ""; // 🔥 Swagger UI at root: https://localhost:xxxx/
});

// 🔥 Map all endpoints automatically
app.MapGroup("/api").MapEndpoints();

app.UseExceptionHandler(); // MUST be before endpoints

// -----------------------------
app.MapControllers();

app.Run();