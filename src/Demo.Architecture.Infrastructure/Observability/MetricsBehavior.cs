using MediatR;
using Prometheus;
using System.Diagnostics;

namespace Demo.Architecture.Infrastructure.Observability;

public class MetricsBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram(
            "mediatr_request_duration_seconds",
            "Duration of MediatR requests",
            new HistogramConfiguration
            {
                LabelNames = new[] { "request_name" }
            });

    private static readonly Counter RequestCounter = Metrics
        .CreateCounter(
            "mediatr_request_total",
            "Total number of MediatR requests",
            new CounterConfiguration
            {
                LabelNames = new[] { "request_name", "status" }
            });

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            RequestDuration
                .WithLabels(requestName)
                .Observe(stopwatch.Elapsed.TotalSeconds);

            RequestCounter
                .WithLabels(requestName, "success")
                .Inc();

            return response;
        }
        catch
        {
            stopwatch.Stop();

            RequestDuration
                .WithLabels(requestName)
                .Observe(stopwatch.Elapsed.TotalSeconds);

            RequestCounter
                .WithLabels(requestName, "error")
                .Inc();

            throw;
        }
    }
}
