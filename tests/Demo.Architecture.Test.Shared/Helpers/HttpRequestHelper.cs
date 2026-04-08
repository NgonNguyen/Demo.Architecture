using System.Net.Http.Json;

namespace Demo.Architecture.Test.Shared.Helpers;

public static class HttpRequestHelper
{
    public static HttpRequestMessage CreateRequest(object body, string key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/products")
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.Add("Idempotency-Key", key);

        return request;
    }
}
