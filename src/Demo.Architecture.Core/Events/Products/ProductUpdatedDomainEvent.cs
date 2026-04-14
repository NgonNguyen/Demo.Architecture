namespace Demo.Architecture.Core.Events.Products;

public class ProductUpdatedDomainEvent(Ulid productId, string name, decimal price) : AppDomainEvent
{
    public Ulid ProductId { get; } = productId;
    public string Name { get; } = name;
    public decimal Price { get; } = price;
}
