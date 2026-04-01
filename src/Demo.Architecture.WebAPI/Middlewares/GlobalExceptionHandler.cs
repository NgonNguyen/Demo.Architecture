using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Title = statusCode == 500
                ? "Internal Server Error"
                : "Request Error",

            Status = statusCode,
            Detail = statusCode == 500
                ? "An unexpected error occurred."
                : exception.Message,

            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] =
            statusCode == 500
                ? "INTERNAL_SERVER_ERROR"
                : "REQUEST_ERROR";

        context.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        });

        return true;
    }
}