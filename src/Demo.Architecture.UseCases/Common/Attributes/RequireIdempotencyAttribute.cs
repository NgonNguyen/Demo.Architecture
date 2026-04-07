namespace Demo.Architecture.UseCases.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequireIdempotencyAttribute : Attribute
{
}
