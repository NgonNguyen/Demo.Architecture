using System.Linq.Expressions;

namespace Demo.Architecture.UseCases.Common.Specifications;

public abstract class AppSpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    public List<Expression<Func<T, object>>> Includes { get; } = new();

    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; protected set; }

    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
}
