using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Rules;

public interface IProductUniquenessChecker
{
    Task<bool> IsNameUnique(string name, CancellationToken ct);
    Task<bool> IsNameUnique(string name, Ulid id, CancellationToken ct);
}
