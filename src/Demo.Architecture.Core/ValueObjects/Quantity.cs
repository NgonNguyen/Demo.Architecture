using Vogen;

namespace Demo.Architecture.Core.ValueObjects;

[ValueObject<int>]
public partial struct Quantity
{
    private static Validation Validate(int value)
        => value > 0
            ? Validation.Ok
            : Validation.Invalid("Quantity must be > 0");
}
