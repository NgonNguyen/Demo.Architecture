using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Services;
using Demo.Architecture.UseCases.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            // ✅ Add MemoryCache
            services.AddMemoryCache();

            // ✅ Replace with MemoryCacheService
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // 🔥 Shared SQLite connection
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

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
