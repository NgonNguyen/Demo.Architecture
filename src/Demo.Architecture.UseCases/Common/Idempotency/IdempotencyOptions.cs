namespace Demo.Architecture.UseCases.Common.Idempotency;

public class IdempotencyOptions
{
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan LockTtl { get; set; } = TimeSpan.FromSeconds(300);
}
