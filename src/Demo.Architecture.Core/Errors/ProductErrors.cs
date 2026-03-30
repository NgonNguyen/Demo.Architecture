namespace Demo.Architecture.Core.Errors;

public static class ProductErrors
{
    public static readonly Error NameRequired =
        new("PRODUCT_NAME_REQUIRED", "Name is required");

    public static readonly Error PriceInvalid =
        new("PRODUCT_PRICE_INVALID", "Price must be > 0");
}
