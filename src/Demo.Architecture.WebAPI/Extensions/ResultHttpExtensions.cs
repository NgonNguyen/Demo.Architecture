using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Architecture.WebAPI.Extensions;

public static class ResultHttpExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return result.Status switch
        {
            ResultStatus.NotFound =>
                Results.NotFound(),

            ResultStatus.Invalid =>
                Results.ValidationProblem(
                    result.ValidationErrors
                        .GroupBy(e => e.Identifier ?? "Error")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                ),

            ResultStatus.Error =>
                Results.Problem(
                    title: "Internal Server Error",
                    detail: string.Empty,
                    statusCode: StatusCodes.Status500InternalServerError
                ),

            _ => Results.BadRequest()
        };
    }

    // 🔥 overload for non-generic Result
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok();

        return result.Status switch
        {
            ResultStatus.NotFound =>
                Results.NotFound(),

            ResultStatus.Invalid =>
                Results.ValidationProblem(
                    result.ValidationErrors
                        .GroupBy(e => e.Identifier ?? "Error")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                ),

            ResultStatus.Error =>
                Results.Problem(
                    title: "Internal Server Error",
                    detail: string.Empty,
                    statusCode: StatusCodes.Status500InternalServerError
                ),

            _ => Results.BadRequest()
        };
    }
}
