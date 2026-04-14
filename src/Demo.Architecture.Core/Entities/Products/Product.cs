using Ardalis.Result;
using Ardalis.SharedKernel;
using Demo.Architecture.Core.Base;
using Demo.Architecture.Core.Errors;
using Demo.Architecture.Core.Events.Products;
using Demo.Architecture.Core.ValueObjects;
using Vogen;

namespace Demo.Architecture.Core.Entities.Products;

public class Product : AppEntityBase<ProductId>, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public Money Price { get; private set; }

    private Product() { }

    private Product(string name, decimal price)
    {
        Id = ProductId.New();
        Name = name;
        Price = Money.From(price);
    }

    public static Result<Product> Create(string name, decimal price)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError { 
                Identifier = ProductErrors.NameRequired.Code, 
                ErrorMessage = ProductErrors.NameRequired.Message });

        if (price <= 0)
            errors.Add(new ValidationError { 
                Identifier = ProductErrors.PriceInvalid.Code, 
                ErrorMessage = ProductErrors.PriceInvalid.Message });

        if (errors.Any())
            return Result.Invalid(errors);

        var newProduct = new Product(name, price);

        newProduct.RegisterDomainEvent(new ProductCreatedDomainEvent(newProduct));

        return Result.Success(newProduct);
    }

    public Result Update(string name, decimal price)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError
            {
                Identifier = ProductErrors.NameRequired.Code,
                ErrorMessage = ProductErrors.NameRequired.Message
            });

        if (price <= 0)
            errors.Add(new ValidationError
            {
                Identifier = ProductErrors.PriceInvalid.Code,
                ErrorMessage = ProductErrors.PriceInvalid.Message
            });

        if (errors.Any())
            return Result.Invalid(errors);

        Name = name;
        Price = Money.From(price);

        RegisterDomainEvent(new ProductUpdatedDomainEvent(Id.Value, Name, Price.Value));

        return Result.Success();
    }
}

[ValueObject<Ulid>]
public partial struct ProductId
{
    public static ProductId New()
        => From(Ulid.NewUlid());
}
