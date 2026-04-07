using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Demo.Architecture.WebAPI.Middlewares;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        logger.LogError(exception,
            "Unhandled Exception. TraceId: {TraceId}", traceId);

        ProblemDetails problemDetails;

        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                problemDetails = new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = exception.Message,
                    Instance = context.Request.Path
                };

                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["errorCode"] = "UNAUTHORIZED";

                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                problemDetails = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred.",
                    Instance = context.Request.Path
                };

                problemDetails.Extensions["traceId"] = traceId;
                problemDetails.Extensions["errorCode"] = "INTERNAL_SERVER_ERROR";

                break;
        }

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        });

        return true;
    }
}