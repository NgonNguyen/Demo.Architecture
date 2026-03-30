using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NUlid;

namespace Demo.Architecture.Infrastructure.Extensions;

public static class VogenPropertyBuilderExtensions
{
    public static PropertyBuilder<TId> HasVogenUlidConversion<TId>(
        this PropertyBuilder<TId> builder)
        where TId : struct
    {
        var type = typeof(TId);
        var fromMethod = type.GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Ulid) },
            null);

        if (fromMethod == null)
        {
            throw new InvalidOperationException(
                $"{type.Name} must have static From(Ulid) method (Vogen).");
        }

        var converter = new ValueConverter<TId, string>(
            v => (string)type.GetProperty("Value")!.GetValue(v)!.ToString()!,
            v => (TId)fromMethod.Invoke(null, new object[] { Ulid.Parse(v) })!
        );

        return builder
            .HasConversion(converter)
            .HasMaxLength(26);
    }

    public static PropertyBuilder<TValue> HasVogenIntConversion<TValue>(
        this PropertyBuilder<TValue> builder)
        where TValue : struct
    {
        return builder.HasConversion(
            v => (int)(typeof(TValue).GetProperty("Value")!.GetValue(v)!),
            v => (TValue)typeof(TValue).GetMethod("From", new[] { typeof(int) })!
                .Invoke(null, new object[] { v })!
        );
    }

    public static PropertyBuilder<TValue> HasVogenDecimalConversion<TValue>(
        this PropertyBuilder<TValue> builder)
        where TValue : struct
    {
        return builder.HasConversion(
            v => (decimal)(typeof(TValue).GetProperty("Value")!.GetValue(v)!),
            v => (TValue)typeof(TValue).GetMethod("From", new[] { typeof(decimal) })!
                .Invoke(null, new object[] { v })!
        );
    }

    public static PropertyBuilder<TValue> HasVogenStringConversion<TValue>(
        this PropertyBuilder<TValue> builder)
        where TValue : struct
    {
        var type = typeof(TValue);

        var valueProp = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{type.Name} must have a public 'Value' property.");

        var fromMethod = type.GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null)
            ?? throw new InvalidOperationException($"{type.Name} must have static From(string) method.");

        var converter = new ValueConverter<TValue, string>(
            v => (string)valueProp.GetValue(v)!,
            v => (TValue)fromMethod.Invoke(null, new object[] { v })!
        );

        return builder.HasConversion(converter);
    }
}
