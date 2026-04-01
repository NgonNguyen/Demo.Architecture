using Demo.Architecture.Infrastructure.Data;
using System.Text.Json;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Common.Json;
using Demo.Architecture.WebAPI.Middlewares;
using Demo.Architecture.WebAPI.OpenApi.Processors;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            context.HttpContext.Request.Path;
    };
});

// -----------------------------
// MediatR
// -----------------------------
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetListProductsQuery).Assembly));

// -----------------------------
// Database (SQLite)
// -----------------------------

builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<AppDbContext>());

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
    config.OperationProcessors.Add(new ProblemDetailsResponseOperationProcessor());
    config.OperationProcessors.Add(new ResponseExampleOperationProcessor());
    config.OperationProcessors.Add(new QueryExampleOperationProcessor());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new UlidJsonConverter());
});

builder.Services.AddOpenTelemetry();

// -----------------------------
var app = builder.Build();

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