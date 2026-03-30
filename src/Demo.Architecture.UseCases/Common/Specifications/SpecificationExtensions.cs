namespace Demo.Architecture.UseCases.Common.Specifications;

public static class SpecificationExtensions
{
    public static IQueryable<T> ApplySpecification<T>(
        this IQueryable<T> query,
        AppSpecification<T> spec)
    {
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        if (spec.OrderBy != null)
            query = spec.OrderBy(query);

        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);

        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        return query;
    }
}
