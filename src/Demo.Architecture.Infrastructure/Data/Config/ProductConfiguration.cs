using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.ValueObjects;
using Demo.Architecture.Infrastructure.Extensions;

namespace Demo.Architecture.Infrastructure.Data.Config;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(p => p.Id)
            .HasVogenUlidConversion<ProductId>()
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasVogenDecimalConversion<Money>()
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
