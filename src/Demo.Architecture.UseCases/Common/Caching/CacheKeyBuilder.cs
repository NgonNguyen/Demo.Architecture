namespace Demo.Architecture.UseCases.Common.Caching;

public static class CacheKeyBuilder
{
    public static string Build(string prefix, params object?[] parts)
    {
        var normalizedParts = parts.Select(Normalize);
        return $"{prefix}:{string.Join(":", normalizedParts)}";
    }

    private static string Normalize(object? input)
    {
        if (input is null) return "null";

        return input switch
        {
            string s => string.IsNullOrWhiteSpace(s)
                ? "all"
                : s.Trim().ToLowerInvariant(),

            DateTime dt => dt.ToUniversalTime().ToString("O"),

            _ => input.ToString()!.Trim().ToLowerInvariant()
        };
    }
}
