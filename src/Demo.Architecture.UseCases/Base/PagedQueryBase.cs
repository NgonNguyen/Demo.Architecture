using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Demo.Architecture.UseCases.Base;

public abstract record PagedQueryBase
{
    [FromQuery(Name = "page")]
    [DefaultValue(1)]
    [Description("Page number (starting from 1)")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    [Description("Page number (starting from 1)")]
    public int PageSize { get; init; } = 20;

    [JsonIgnore]
    [OpenApiIgnore]
    public int Skip => (Page - 1) * PageSize;

    [FromQuery(Name = "searchTerm")]
    [DefaultValue("")]
    [Description("Search keyword")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "sort")]
    [DefaultValue("name")]
    [Description("Sorting")]
    public string? Sort { get; init; }
}
