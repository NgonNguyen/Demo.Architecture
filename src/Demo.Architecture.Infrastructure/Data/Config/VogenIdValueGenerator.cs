using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NUlid;

namespace Demo.Architecture.Infrastructure.Data.Config;

public class VogenIdValueGenerator<TId> : ValueGenerator<TId>
{
    public override bool GeneratesTemporaryValues => false;

    public override TId Next(EntityEntry entry)
    {
        var ulid = Ulid.NewUlid();

        var fromMethod = typeof(TId).GetMethod("From", new[] { typeof(Ulid) });

        if (fromMethod == null)
            throw new InvalidOperationException($"{typeof(TId).Name} is not a valid Vogen type");

        return (TId)fromMethod.Invoke(null, new object[] { ulid })!;
    }
}