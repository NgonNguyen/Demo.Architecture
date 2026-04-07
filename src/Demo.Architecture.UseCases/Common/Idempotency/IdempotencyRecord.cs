namespace Demo.Architecture.UseCases.Common.Idempotency;

public class IdempotencyRecord
{
    public string RequestHash { get; set; } = default!;
    public string ResponseJson { get; set; } = default!;
    public int StatusCode { get; set; }
}
