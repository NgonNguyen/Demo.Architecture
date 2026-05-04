using Ardalis.SharedKernel;
using Demo.Architecture.Core.Base.Interfaces;
using Demo.Architecture.Core.Entities.Orders;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Identity;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;

namespace Demo.Architecture.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext, IReadOnlyApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator, ICurrentUser currentUser)
      : base(options)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    IQueryable<Product> IReadOnlyApplicationDbContext.Products => Set<Product>().AsNoTracking();
    IQueryable<Order> IReadOnlyApplicationDbContext.Orders => Set<Order>().AsNoTracking();

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

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var userId = _currentUser?.GetUser()?.UserId ?? "system";

        var domainEntities = ChangeTracker
            .Entries<HasDomainEventsBase>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditable auditable)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    auditable.CreatedBy = userId;
                    auditable.CreatedAt = now;
                    break;

                case EntityState.Modified:
                    auditable.UpdatedBy = userId;
                    auditable.UpdatedAt = now;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(ct);

        foreach (var entity in domainEntities)
        {
            var events = entity.DomainEvents.ToArray();

            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _mediator.Publish((dynamic)domainEvent, ct);
            }
        }

        return result;
    }

    private static void SetIsActiveFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IActivatable
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.IsActive);
    }
}
