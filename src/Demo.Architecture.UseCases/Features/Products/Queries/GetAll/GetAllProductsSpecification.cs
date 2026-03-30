using Ardalis.Specification;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Specifications;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetAll;

public class GetAllProductsSpecification : AppSpecification<Product>
{
    public GetAllProductsSpecification(string? searchTerm, string? sort)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            Criteria = p => p.Name.ToLower().Contains(term);
        }

        OrderBy = sort?.ToLower() switch
        {
            "name" => q => q.OrderBy(x => x.Name),
            "name_desc" => q => q.OrderByDescending(x => x.Name),
            "price" => q => q.OrderBy(x => x.Price),
            "price_desc" => q => q.OrderByDescending(x => x.Price),
            _ => q => q.OrderBy(x => x.Name)
        };
    }
}
