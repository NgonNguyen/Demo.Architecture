using Demo.Architecture.Core.Entities.Orders;
using Demo.Architecture.Core.Entities.Products;

namespace Demo.Architecture.UseCases.Common.Interfaces;

public interface IReadOnlyApplicationDbContext
{
    IQueryable<Product> Products { get; }
    IQueryable<Order> Orders { get; }
}
