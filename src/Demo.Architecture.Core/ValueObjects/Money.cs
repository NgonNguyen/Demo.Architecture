using Vogen;

namespace Demo.Architecture.Core.ValueObjects;

[ValueObject<decimal>]
public partial struct Money
{
    private static Validation Validate(decimal value)
        => value >= 0
            ? Validation.Ok
            : Validation.Invalid("Price cannot be negative");
}
