using NUlid;
using System.Text.Json.Serialization;

namespace Demo.Architecture.UseCases.Features.Coffees.Queries.GetList;

//public class GetListCoffeesResponse
//{
//    public int Id { get; set; }

//    public string Title { get; set; } = default!;

//    public string Description { get; set; } = default!;

//    public string Image { get; set; } = default!;
//}


public record GetListCoffeesResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("image")] string Image
);