using MassTransit;

namespace Demo.Architecture.WebAPI.Consumer.Observers;

public class ConsoleRetryObserver : IRetryObserver
{
    Task IRetryObserver.PostCreate<T>(RetryPolicyContext<T> context)
    {
        return Task.CompletedTask;
    }

    Task IRetryObserver.PostFault<T>(RetryContext<T> context)
    {
        return Task.CompletedTask;
    }

    Task IRetryObserver.PreRetry<T>(RetryContext<T> context)
    {
        var delaySeconds = context.Delay.HasValue ? context.Delay.Value.TotalSeconds : 0;

        Console.WriteLine(
            $"[Retry] Attempt #{context.RetryAttempt} after {delaySeconds} seconds. " +
            $"Exception: {context.Exception.Message}");

        return Task.CompletedTask;
    }

    Task IRetryObserver.RetryComplete<T>(RetryContext<T> context)
    {
        throw new NotImplementedException();
    }

    Task IRetryObserver.RetryFault<T>(RetryContext<T> context)
    {
        Console.WriteLine(
            $"[Retry Fault] Message failed after {context.RetryAttempt} attempts. " +
            $"Exception: {context.Exception.Message}");

        return Task.CompletedTask;
    }
}

