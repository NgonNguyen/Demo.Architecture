using MediatR;
using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Update;

public record UpdateProductCommand(
    Ulid Id,  
    string Name,
    decimal Price
) : IRequest<Result>;
