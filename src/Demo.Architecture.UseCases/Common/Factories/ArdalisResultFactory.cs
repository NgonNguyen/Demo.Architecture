namespace Demo.Architecture.UseCases.Common.Factories;

public static class ArdalisResultFactory
{
    public static object CreateInvalid(Type responseType, IEnumerable<ValidationError> errors)
    {
        // ✅ Handle Result<T>
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];

            var method = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result<object>.Invalid),
                    new[] { typeof(IEnumerable<ValidationError>) });

            return method!.Invoke(null, new object[] { errors })!;
        }

        // ✅ Handle non-generic Result
        if (responseType == typeof(Result))
        {
            return Result.Invalid(errors);
        }

        throw new InvalidOperationException(
            $"TResponse must be Result or Result<T>. Actual: {responseType.Name}");
    }
}
