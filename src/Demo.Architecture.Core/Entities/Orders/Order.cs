using Ardalis.SharedKernel;
using Demo.Architecture.Core.Base;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.ValueObjects;
using Vogen;

namespace Demo.Architecture.Core.Entities.Orders;

public class Order : AppEntityBase<OrderId>, IAggregateRoot
{
    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public DateTime OrderDate { get; private set; }

    public Order(DateTime orderDate)
    {
        OrderDate = orderDate;
    }

    public void AddItem(ProductId productId, Quantity quantity, Money unitPrice)
    {
        var item = new OrderItem(Id, productId, quantity, unitPrice);
        _items.Add(item);
    }

    public void RemoveItem(OrderItemId itemId)
    {
        var item = _items.FirstOrDefault(x => x.Id == itemId);
        if (item is null) return;

        _items.Remove(item);
    }

    public decimal GetTotalAmount()
    {
        return _items.Sum(x => x.Quantity.Value * x.UnitPrice.Value);
    }
}

[ValueObject<Ulid>]
public partial struct OrderId;