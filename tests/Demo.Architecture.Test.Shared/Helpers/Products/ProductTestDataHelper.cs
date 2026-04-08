using Demo.Architecture.Test.Shared.Constants;
using Demo.Architecture.UseCases.Features.Products.Commands.Create;
using Demo.Architecture.UseCases.Features.Products.Commands.Update;
using Demo.Architecture.UseCases.Features.Products.Queries.GetList;
using NUlid;
using System.Net.Http.Json;
using AppModels = Demo.Architecture.UseCases.Common.Models;

namespace Demo.Architecture.Test.Shared.Helpers.Products;

public static class ProductTestDataHelper
{
    public static AppModels.PagedResult<GetListProductsResponse> CreatePagedResponse()
    {
        return new AppModels.PagedResult<GetListProductsResponse>(
            new List<GetListProductsResponse>
            {
                new(
                    Ulid.NewUlid(),
                    TestConstants.ValidProductNameA,
                    TestConstants.ValidPriceA)
            },
            1,
            1,
            20
        );
    }

    public static HttpRequestMessage CreateRequest(CreateProductCommand command, string? key = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, TestConstants.ProductsEndpoint)
        {
            Content = JsonContent.Create(command)
        };

        request.Headers.Add("Idempotency-Key", key ?? Ulid.NewUlid().ToString());

        return request;
    }

    public static HttpRequestMessage UpdateRequest(UpdateProductCommand command, string? key = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{TestConstants.ProductsEndpoint}/{command.Id}")
        {
            Content = JsonContent.Create(command)
        };

        request.Headers.Add("Idempotency-Key", key ?? Ulid.NewUlid().ToString());

        return request;
    }

    public static HttpRequestMessage DeleteRequest(Ulid id, string? key = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{TestConstants.ProductsEndpoint}/{id}");

        request.Headers.Add("Idempotency-Key", key ?? Ulid.NewUlid().ToString());

        return request;
    }
}
