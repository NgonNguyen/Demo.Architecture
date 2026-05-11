using Demo.Architecture.Shared.Models;

namespace Demo.Architecture.UseCases.ExternalServices;

public interface ICoffeeApiClient
{
    Task<List<CoffeeResponse>> GetHotCoffeeAsync(
        CancellationToken cancellationToken);
}