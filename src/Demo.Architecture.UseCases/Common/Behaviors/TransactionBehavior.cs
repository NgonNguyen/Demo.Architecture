using Demo.Architecture.UseCases.Common.Interfaces;
using Demo.Architecture.UseCases.Common.Messaging.Commands;
using MediatR;

namespace Demo.Architecture.UseCases.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    IApplicationDbContext context)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Optional: only apply to Commands
        if (request is not ICommand<TResponse>)
            return await next();

        var response = await next(cancellationToken);

        // Optional: only commit when success
        if (response is IResult result && result.Status == ResultStatus.Ok)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
