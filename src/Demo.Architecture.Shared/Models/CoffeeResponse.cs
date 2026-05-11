namespace Demo.Architecture.Shared.Models;

public class CoffeeResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Image { get; set; } = default!;
}
