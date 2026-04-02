using MediatR;
using NUlid;

namespace Demo.Architecture.UseCases.Features.Products.Commands.Delete;

public record DeleteProductCommand(Ulid Id) : IRequest<Result>;
