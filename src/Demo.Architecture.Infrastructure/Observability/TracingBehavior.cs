using MediatR;
using System.Diagnostics;

namespace Demo.Architecture.Infrastructure.Observability;

public class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly ActivitySource ActivitySource =
        new("Demo.Architecture");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = ActivitySource.StartActivity(requestName);

        activity?.SetTag("request.name", requestName);
        activity?.SetTag("request.type", typeof(TRequest).FullName);

        try
        {
            var response = await next();

            activity?.SetTag("status", "OK");

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetTag("status", "ERROR");
            activity?.SetTag("exception", ex.Message);

            throw;
        }
    }
}
