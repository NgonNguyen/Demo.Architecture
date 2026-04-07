using Demo.Architecture.Core.Errors;

namespace Demo.Architecture.UseCases.Common.Errors;

public static class CommonErrors
{
    public static readonly Error IdempotencyKeyRequired =
        new("IDEMPOTENCY_KEY_REQUIRED", "Idempotency-Key header is required");

    public static readonly Error IdempotencyConflict =
        new("IDEMPOTENCY_CONFLICT", "Idempotency key reused with different payload");

    public static readonly Error IdempotencyInProgress =
        new("IDEMPOTENCY_IN_PROGRESS", "Request is already in progress");
}
