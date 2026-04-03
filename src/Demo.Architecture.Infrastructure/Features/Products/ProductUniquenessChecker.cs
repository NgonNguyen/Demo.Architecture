using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.UseCases.Features.Products.Rules;
using NUlid;

namespace Demo.Architecture.Infrastructure.Features.Products;

public class ProductUniquenessChecker : IProductUniquenessChecker
{
    private readonly AppDbContext _context;

    public ProductUniquenessChecker(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsNameUnique(string name, CancellationToken ct)
    {
        return !await _context.Products.AnyAsync(p => p.Name == name, ct);
    }

    public async Task<bool> IsNameUnique(string name, Ulid id, CancellationToken ct)
    {
        return !await _context.Products
            .AnyAsync(p => p.Name == name && p.Id != id, ct);
    }
}
