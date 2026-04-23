using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Infrastructure.Features.Products;
using Demo.Architecture.Test.Shared.Services;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Idempotency;
using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Features.Products.Rules;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Demo.Architecture.Test.Shared.Web;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _connection = default!;
    private Action<IServiceCollection>? _configureServices;

    // ✅ ADD THIS METHOD
    public TestWebApplicationFactory WithServices(Action<IServiceCollection> configureServices)
    {
        _configureServices = configureServices;
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // ❌ Remove Redis cache service
            services.RemoveAll<ICacheService>();

            // ❌ (Optional but recommended) remove Redis connection too
            services.RemoveAll<IConnectionMultiplexer>();
            services.RemoveAll<IIdempotencyService>();

            services.RemoveAll(typeof(IBus));
            services.RemoveAll(typeof(IPublishEndpoint));
            services.RemoveAll(typeof(ISendEndpointProvider));
            services.RemoveAll(typeof(IBusControl));

            // 🔥 Remove hosted service (very important)
            services.RemoveAll<IHostedService>();

            // 🔥 Remove health checks (root cause)
            services.RemoveAll<IHealthCheck>();

            var healthChecks = services
                .Where(d => d.ServiceType.Name.Contains("HealthCheck"))
                .ToList();

            foreach (var hc in healthChecks)
            {
                if (hc.ImplementationInstance?.ToString()?.Contains("MassTransit") == true ||
                    hc.ImplementationType?.Name.Contains("MassTransit") == true)
                {
                    services.Remove(hc);
                }
            }

            // ✅ Add MemoryCache
            services.AddMemoryCache();
            
            services.AddSingleton<IIdempotencyService, InMemoryIdempotencyService>();

            // ✅ Replace with MemoryCacheService
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(Program).Assembly); // or your consumers assembly

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            // 🔥 Shared SQLite connection
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddScoped<IProductUniquenessChecker, ProductUniquenessChecker>();

            services.AddValidatorsFromAssembly(typeof(IProductUniquenessChecker).Assembly);

            services.AddHttpContextAccessor();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // 🔥 Apply custom overrides (THIS is what WithServices uses)
            _configureServices?.Invoke(services);

            // Build provider
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _connection?.Close();
        _connection?.Dispose();
    }
}
