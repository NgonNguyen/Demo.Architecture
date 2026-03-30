using Demo.Architecture.UseCases.Base;
using MediatR;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetAll;

public record GetAllProductsQuery
    : PagedQueryBase, IRequest<Result<AppModels.PagedResult<GetAllProductsResponse>>>;
