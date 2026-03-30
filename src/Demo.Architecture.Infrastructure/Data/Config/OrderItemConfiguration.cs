using Demo.Architecture.Core.Entities.Orders;
using Demo.Architecture.Core.ValueObjects;
using Demo.Architecture.Infrastructure.Extensions;

namespace Demo.Architecture.Infrastructure.Data.Config;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasVogenUlidConversion()
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasVogenUlidConversion();

        builder.Property(x => x.ProductId)
            .HasVogenUlidConversion();

        builder.Property(x => x.Quantity)
            .HasVogenIntConversion<Quantity>();

        builder.Property(x => x.UnitPrice)
            .HasVogenDecimalConversion<Money>()
            .HasColumnType("decimal(18,2)");
    }
}
