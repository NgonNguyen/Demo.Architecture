using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.UseCases.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.WebAPI.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        services.AddScoped<IApplicationDbContext>(
            sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IReadOnlyApplicationDbContext>(
            sp => sp.GetRequiredService<AppDbContext>());

        var dbPath = Path.Combine(
            builder.Environment.ContentRootPath,
            "Data",
            "app.db");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        return services;
    }
}
