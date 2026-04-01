using Demo.Architecture.UseCases.Features.Products.Errors;

namespace Demo.Architecture.WebAPI.Common.Errors;

public static class ErrorMapping
{
    private static readonly Dictionary<string, string> Errors = new()
    {
        { ProductErrors.NotFound, "Product not found" }
    };

    public static string GetMessage(string code)
    {
        return Errors.TryGetValue(code, out var message)
            ? message
            : "Unknown error";
    }
}
