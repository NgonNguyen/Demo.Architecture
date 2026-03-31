namespace Demo.Architecture.UseCases.Common.Models;

using System.Text.Json.Serialization;

public class PagedResult<T>
{
    [property: JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; }

    [property: JsonPropertyName("totalCount")]
    public int TotalCount { get; }

    [property: JsonPropertyName("page")]
    public int Page { get; }

    [property: JsonPropertyName("pageSize")]
    public int PageSize { get; }

    public PagedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
