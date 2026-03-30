//using Ardalis.SharedKernel;
//using Demo.Architecture.Infrastructure.Data;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Text;

namespace Demo.Architecture.Infrastructure;

public static class ProgramExtensions
{
    //private static void AddDbContextWithSqlite(IServiceCollection services, IConfiguration configuration)
    //{
    //    // services.AddScoped<EventDispatchInterceptor>();
    //    services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
    //    var connectionString = configuration.GetConnectionString("SqliteConnection");
    //    services.AddDbContext<AppDbContext>((provider, options) =>
    //    {
    //        options.UseSqlite("Data Source=Data/app.db");

    //        // options.UseSqlite(connectionString);
    //        //.AddMetronomeDbTracking(provider)
    //        //.AddInterceptors(provider.GetRequiredService<EventDispatchInterceptor>());
    //    });

    //}
}
