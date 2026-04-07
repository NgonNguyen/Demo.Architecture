using Ardalis.Result;
using Demo.Architecture.WebAPI.Common.Errors;
using Demo.Architecture.WebAPI.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Architecture.WebAPI.Extensions;

public static class ResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        var code = result.Errors?.FirstOrDefault() ?? "UNKNOWN";
        var message = ErrorMapping.GetMessage(code);

        return result.Status switch
        {
            ResultStatus.NotFound => new ProblemHttpResult(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = message,
                    Extensions =
                    {
                        ["errorCode"] = code
                    }
                }),

            ResultStatus.Invalid =>
                // Return ProblemDetails with validation errors and error codes so the
                // front-end can map codes to localized messages. We include an
                // "errors" extension where each key is the field/identifier and the
                // value is an array of objects { message, code }.
                new ProblemHttpResult(
                    new ProblemDetails
                    {
                        Title = "Validation Error",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "One or more validation errors occurred.",
                        Extensions =
                        {
                            ["errors"] = result.ValidationErrors.Select(ev => new
                            {
                                field = string.IsNullOrEmpty(ev.Identifier)
                                    ? "unknown"
                                    : ev.Identifier,

                                message = ev.ErrorMessage,

                                code = string.IsNullOrEmpty(ev.ErrorCode)
                                    ? $"{ev.Identifier?.ToUpperInvariant()}_VALIDATION_ERROR"
                                    : ev.ErrorCode
                            }).ToArray()
                        }
                    }
                ),

            ResultStatus.Error => new ProblemHttpResult(
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred.",
                    Extensions =
                    {
                        ["errorCode"] = "INTERNAL_SERVER_ERROR"
                    }
                }),

            _ => Results.BadRequest()
        };
    }

    // Overload for non-generic Result (e.g., from DELETE operations)
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent(); // 204 for successful DELETE operations

        return result.Status switch
        {
            ResultStatus.NotFound => new ProblemHttpResult(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Resource not found",
                    Extensions =
                    {
                        ["errorCode"] = "NOT_FOUND"
                    }
                }),

            ResultStatus.Invalid =>
                new ProblemHttpResult(
                    new ProblemDetails
                    {
                        Title = "Validation Error",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "One or more validation errors occurred.",
                        Extensions =
                        {
                            ["errors"] = result.ValidationErrors.Select(ev => new
                            {
                                field = string.IsNullOrEmpty(ev.Identifier)
                                    ? "unknown"
                                    : ev.Identifier,

                                message = ev.ErrorMessage,

                                code = string.IsNullOrEmpty(ev.ErrorCode)
                                    ? $"{ev.Identifier?.ToUpperInvariant()}_VALIDATION_ERROR"
                                    : ev.ErrorCode
                            }).ToArray()
                        }
                    }
                ),

            ResultStatus.Error => new ProblemHttpResult(
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred.",
                    Extensions =
                    {
                        ["errorCode"] = "INTERNAL_SERVER_ERROR"
                    }
                }),

            _ => Results.BadRequest()
        };
    }
}
