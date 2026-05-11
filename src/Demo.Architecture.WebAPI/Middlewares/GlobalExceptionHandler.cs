using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Timeout;
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
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                problemDetails = CreateProblemDetails(
                    title: "Unauthorized",
                    detail: exception.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "UNAUTHORIZED");

                break;

            case TimeoutRejectedException:
                context.Response.StatusCode =
                    StatusCodes.Status504GatewayTimeout;

                problemDetails = CreateProblemDetails(
                    title: "Gateway Timeout",
                    detail: "External API request timed out.",
                    statusCode: StatusCodes.Status504GatewayTimeout,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "EXTERNAL_API_TIMEOUT");

                break;

            case BrokenCircuitException:
                context.Response.StatusCode =
                    StatusCodes.Status503ServiceUnavailable;

                problemDetails = CreateProblemDetails(
                    title: "Service Unavailable",
                    detail: "External API is temporarily unavailable.",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "CIRCUIT_BREAKER_OPEN");

                break;

            case RateLimiterRejectedException:
                context.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                problemDetails = CreateProblemDetails(
                    title: "Too Many Requests",
                    detail: "Too many outgoing requests to external service.",
                    statusCode: StatusCodes.Status429TooManyRequests,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "RATE_LIMIT_EXCEEDED");

                break;

            case HttpRequestException:
                context.Response.StatusCode =
                    StatusCodes.Status503ServiceUnavailable;

                problemDetails = CreateProblemDetails(
                    title: "External Service Error",
                    detail: "Unable to connect to external service.",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "EXTERNAL_SERVICE_ERROR");

                break;

            case TaskCanceledException:
                context.Response.StatusCode =
                    StatusCodes.Status504GatewayTimeout;

                problemDetails = CreateProblemDetails(
                    title: "Request Timeout",
                    detail: "The request was cancelled or timed out.",
                    statusCode: StatusCodes.Status504GatewayTimeout,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "REQUEST_TIMEOUT");

                break;

            default:
                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                problemDetails = CreateProblemDetails(
                    title: "Internal Server Error",
                    detail: "An unexpected error occurred.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    instance: context.Request.Path,
                    traceId: traceId,
                    errorCode: "INTERNAL_SERVER_ERROR");

                break;
        }

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        });

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        string title,
        string detail,
        int statusCode,
        string? instance,
        string traceId,
        string errorCode)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = instance
        };

        problemDetails.Extensions["traceId"] = traceId;

        problemDetails.Extensions["errorCode"] = errorCode;

        return problemDetails;
    }
}