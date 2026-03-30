using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetAll;

public record GetAllProductsResponse(
    Ulid Id,
    string Name,
    decimal Price
);
