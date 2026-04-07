using Demo.Architecture.UseCases.Common.Factories;
using FluentValidation;
using MediatR;

namespace Demo.Architecture.UseCases.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var validationErrors = failures.Select(f => new ValidationError
            {
                Identifier = f.PropertyName,
                ErrorMessage = f.ErrorMessage,
                ErrorCode = string.IsNullOrEmpty(f.ErrorCode)
                    ? $"{f.PropertyName.ToUpperInvariant()}_VALIDATION_ERROR"
                    : f.ErrorCode
            });

            var result = ArdalisResultFactory.CreateInvalid(typeof(TResponse), validationErrors);

            return (TResponse)result;
        }

        return await next();
    }
}
