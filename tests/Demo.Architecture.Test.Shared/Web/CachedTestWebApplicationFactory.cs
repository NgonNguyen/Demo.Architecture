using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Services;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StackExchange.Redis;

namespace Demo.Architecture.Test.Shared.Web;

public class CachedTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.RemoveAll<ICacheService>();
            services.RemoveAll<IConnectionMultiplexer>();

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));

            services.RemoveAll<IIntegrationEventPublisher>();

            var mockPublisher = new Mock<IIntegrationEventPublisher>();
            services.AddSingleton(mockPublisher.Object);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
