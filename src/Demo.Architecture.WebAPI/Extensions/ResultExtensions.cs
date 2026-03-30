using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Architecture.WebAPI.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        if (result.Status == ResultStatus.NotFound)
            return new NotFoundResult();

        if (result.Status == ResultStatus.Invalid)
            return new BadRequestObjectResult(result.ValidationErrors);

        if (result.Status == ResultStatus.Error)
            return new ObjectResult(result.Errors)
            {
                StatusCode = 500
            };

        return new BadRequestResult();
    }

    // 🔥 Add this overload
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        if (result.Status == ResultStatus.NotFound)
            return new NotFoundResult();

        if (result.Status == ResultStatus.Invalid)
            return new BadRequestObjectResult(result.ValidationErrors);

        if (result.Status == ResultStatus.Error)
            return new ObjectResult(result.Errors)
            {
                StatusCode = 500
            };

        return new BadRequestResult();
    }
}
