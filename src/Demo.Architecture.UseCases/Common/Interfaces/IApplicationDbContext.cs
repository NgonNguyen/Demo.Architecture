using Demo.Architecture.Core.Entities.Orders;
using Demo.Architecture.Core.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.UseCases.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
