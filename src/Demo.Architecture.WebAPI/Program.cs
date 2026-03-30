using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Queries.GetAll;
using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Common.Json;
using Demo.Architecture.WebAPI.Middlewares;
using Demo.Architecture.WebAPI.OpenApi.Processors;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // optional

// -----------------------------
// MediatR
// -----------------------------
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly));

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

    /*config.PostProcess = d =>
    {
        d.Info.TermsOfService = "Term of service";
        d.Info.Version = "v1.0";
        d.Info.License = new OpenApiLicense
        {
            Url = "http://www.apache.org/licenses/LICENSE-2.0.html",
            Name = "Apache 2.0"
        };
    };
    config.AddSecurity("openId", [], new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = "https://localhost/.well-known/openid-configuration",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Standard authorisation using the Bearer scheme. Example: \"bearer {token}\"",
    });*/
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new UlidJsonConverter());
});

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