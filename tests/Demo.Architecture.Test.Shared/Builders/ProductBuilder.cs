using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Test.Shared.Constants;

namespace Demo.Architecture.Test.Shared.Builders;

public class ProductBuilder
{
    private string _name = TestConstants.ValidProductNameA;
    private decimal _price = TestConstants.ValidPriceA;

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public Product Build()
    {
        return Product.Create(_name, _price);
    }
}
