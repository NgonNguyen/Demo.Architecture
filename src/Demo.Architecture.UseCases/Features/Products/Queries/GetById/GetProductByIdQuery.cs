using Demo.Architecture.Core.Entities.Products;
using MediatR;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetById;

public sealed record GetProductByIdQuery(ProductId Id) : IRequest<Result<GetProductByIdResponse>>;
