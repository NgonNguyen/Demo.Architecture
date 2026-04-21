using Demo.Architecture.Infrastructure;
using Demo.Architecture.Infrastructure.Caching;
using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Infrastructure.Features.Products;
using Demo.Architecture.Infrastructure.Messaging.Extensions;
using Demo.Architecture.Infrastructure.Messaging.RabbitMQ;
using Demo.Architecture.Infrastructure.Observability;
using Demo.Architecture.Shared.Serialization;
using Demo.Architecture.UseCases;
using Demo.Architecture.UseCases.Common.Behaviors;
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
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "web-api"))
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

builder.Services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

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
    builder.Services.AddIdempotency();  
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
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

app.UseHttpMetrics();
app.MapMetrics();

// -----------------------------
// Middleware
// -----------------------------
app.UseHttpsRedirection();

app.UseAuthorization();

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