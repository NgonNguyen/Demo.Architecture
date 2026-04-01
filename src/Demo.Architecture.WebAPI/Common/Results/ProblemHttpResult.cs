using Microsoft.AspNetCore.Mvc;

namespace Demo.Architecture.WebAPI.Common.Results;

public class ProblemHttpResult : IResult
{
    private readonly ProblemDetails _problem;

    public ProblemHttpResult(ProblemDetails problem)
    {
        _problem = problem;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var service = httpContext.RequestServices
            .GetRequiredService<IProblemDetailsService>();

        httpContext.Response.StatusCode = _problem.Status ?? StatusCodes.Status500InternalServerError;

        await service.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = _problem
        });
    }
}
