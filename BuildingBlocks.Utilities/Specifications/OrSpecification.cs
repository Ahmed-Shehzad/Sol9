using System.Linq.Expressions;

namespace BuildingBlocks.Utilities.Specifications;

/// <summary>
/// Represents a logical OR specification for entities of type T.
/// </summary>
/// <typeparam name="T">The type of entities to be specified.</typeparam>
public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrSpecification{T}"/> class.
    /// </summary>
    /// <param name="left">The left specification to be ORed.</param>
    /// <param name="right">The right specification to be ORed.</param>
    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    /// <summary>
    /// Converts the specification into a LINQ expression.
    /// </summary>
    /// <returns>A LINQ expression representing the ORed specifications.</returns>
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var body = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}