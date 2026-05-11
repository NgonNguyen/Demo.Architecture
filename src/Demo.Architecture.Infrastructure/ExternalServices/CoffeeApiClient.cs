using Demo.Architecture.Shared.Models;
using Demo.Architecture.UseCases.ExternalServices;
using System.Net.Http.Json;

namespace Demo.Architecture.Infrastructure.ExternalServices;

public class CoffeeApiClient : ICoffeeApiClient
{
    private readonly HttpClient _httpClient;

    public CoffeeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CoffeeResponse>> GetHotCoffeeAsync(
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            "/coffee/hot",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<
                List<CoffeeResponse>>(
                    cancellationToken: cancellationToken);

        return result ?? [];
    }
}
