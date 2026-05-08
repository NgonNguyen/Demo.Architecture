using Demo.Architecture.WebAPI.Common.Endpoints;
using Demo.Architecture.WebAPI.Middlewares;
using Prometheus;

namespace Demo.Architecture.WebAPI.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseApplicationMiddlewares(
        this WebApplication app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        app.UseHttpMetrics();

        app.MapMetrics();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseMiddleware<UserLoggingMiddleware>();

        if (app.Environment.IsDevelopment() ||
            app.Environment.IsStaging())
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
            settings.Path = "";
        });

        app.UseExceptionHandler();

        app.MapGroup("/api").MapEndpoints();

        app.MapControllers();

        return app;
    }
}
