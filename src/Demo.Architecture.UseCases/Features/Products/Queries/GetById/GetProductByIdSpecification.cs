using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.UseCases.Common.Specifications;

namespace Demo.Architecture.UseCases.Features.Products.Queries.GetById;

public class GetProductByIdSpecification : AppSpecification<Product>
{
    public GetProductByIdSpecification(ProductId id)
    {
        Criteria = p => p.Id == id;
        // Query.Where(p => p.Id == id);

        // Optional: include related data if needed later
        // Query.Include(p => p.Category);
    }
}
