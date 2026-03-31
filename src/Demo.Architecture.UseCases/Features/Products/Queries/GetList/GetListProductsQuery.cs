using Demo.Architecture.UseCases.Base;
using MediatR;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetList;

public record GetListProductsQuery
    : PagedQueryBase, IRequest<Result<AppModels.PagedResult<GetListProductsResponse>>>;
