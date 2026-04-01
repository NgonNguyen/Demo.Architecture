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

            ResultStatus.Invalid => Results.ValidationProblem(
                result.ValidationErrors
                    .GroupBy(e => e.Identifier ?? "Error")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
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
