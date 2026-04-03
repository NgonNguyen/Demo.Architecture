using FluentValidation.Results;

namespace Demo.Architecture.UseCases.Common.Exceptions;

public class ApiValidationException : Exception
{
    public IEnumerable<ValidationFailure> Failures { get; }

    public ApiValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Failures = failures;
    }
}
