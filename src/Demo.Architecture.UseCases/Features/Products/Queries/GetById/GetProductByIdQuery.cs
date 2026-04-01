using Demo.Architecture.Core.Entities.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetById;

public sealed record GetProductByIdQuery(
    [FromRoute(Name = "id")]
    [Description("An identifier for a product.")]
    ProductId Id
) : IRequest<Result<GetProductByIdResponse>>;
