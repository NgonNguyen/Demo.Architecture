using Demo.Architecture.Core.Entities.Products;

namespace Demo.Architecture.Core.Events.Products;

public class ProductCreatedDomainEvent(Product product) : AppDomainEvent
{
    public Product Product { get; } = product;
}
