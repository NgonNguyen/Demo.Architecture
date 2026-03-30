using Demo.Architecture.Core.Base;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.ValueObjects;
using Vogen;

namespace Demo.Architecture.Core.Entities.Orders;

public class OrderItem : AppEntityBase<OrderItemId>
{
    public OrderId OrderId { get; private set; }
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

    internal OrderItem(OrderId orderId, 
        ProductId productId, 
        Quantity quantity, 
        Money unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void UpdateQuantity(Quantity quantity)
    {
        Quantity = quantity;
    }

    public decimal GetTotal() => Quantity.Value * UnitPrice.Value;
}

[ValueObject<Ulid>]
public partial struct OrderItemId;
