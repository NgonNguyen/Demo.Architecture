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
                            ["errors"] = result.ValidationErrors
                                .Select(ev =>
                                {
                                    // code is the identifier when available
                                    var codeVal = ev.Identifier ?? "Error";

                                    // derive field name from error code when possible (eg. PRODUCT_PRICE_INVALID -> price)
                                    string field;
                                    if (!string.IsNullOrEmpty(ev.Identifier) && ev.Identifier.Contains("_"))
                                    {
                                        var parts = ev.Identifier.Split('_');
                                        if (parts.Length >= 2)
                                            field = parts[1].ToLowerInvariant();
                                        else
                                            field = "Error";
                                    }
                                    else
                                    {
                                        field = "Error";
                                    }

                                    return new
                                    {
                                        field,
                                        message = ev.ErrorMessage,
                                        code = codeVal
                                    };
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
