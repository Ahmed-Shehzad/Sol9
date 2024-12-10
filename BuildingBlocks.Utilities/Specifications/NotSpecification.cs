using System.Linq.Expressions;

namespace BuildingBlocks.Utilities.Specifications;

/// <summary>
/// Represents a logical NOT operation on a specification.
/// </summary>
/// <typeparam name="T">The type of the entities to be evaluated.</typeparam>
public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotSpecification{T}"/> class.
    /// </summary>
    /// <param name="specification">The specification to be negated.</param>
    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    /// <summary>
    /// Converts the specification into a LINQ expression.
    /// </summary>
    /// <returns>A LINQ expression representing the logical NOT operation on the original specification.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var body = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}