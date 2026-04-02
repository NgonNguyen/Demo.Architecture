using Demo.Architecture.Core.Base;
using Demo.Architecture.Core.Base.Interfaces;
using Demo.Architecture.Core.Entities.Orders;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Interfaces;

namespace Demo.Architecture.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IActivatable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetIsActiveFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        //configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SetIsActiveFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IActivatable
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.IsActive);
    }
}
