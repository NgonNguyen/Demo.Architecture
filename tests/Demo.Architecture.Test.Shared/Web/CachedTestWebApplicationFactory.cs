using Demo.Architecture.Test.Shared.Services;
using Demo.Architecture.UseCases.Common.Behaviors;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Demo.Architecture.Test.Shared.Web;

public class CachedTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove Redis
            services.RemoveAll<ICacheService>();
            services.RemoveAll<IConnectionMultiplexer>();

            // Add MemoryCache
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // ✅ IMPORTANT: Add CachingBehavior back
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
        });
    }
}
