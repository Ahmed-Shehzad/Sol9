using System.Linq.Expressions;
using BuildingBlocks.Contracts.Types;

namespace BuildingBlocks.Utilities.Specifications;

/// <summary>
/// Represents a specification for filtering and querying data.
/// </summary>
/// <typeparam name="T">The type of the data to be filtered.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// Converts the specification into a LINQ expression.
    /// </summary>
    /// <returns>An expression tree representing the specification.</returns>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Creates a new specification that represents the logical AND operation between the current specification and another one.
    /// </summary>
    /// <param name="specification">The specification to be combined with the current one.</param>
    /// <returns>A new specification representing the logical AND operation.</returns>
    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    /// <summary>
    /// Creates a new specification that represents the logical OR operation between the current specification and another one.
    /// </summary>
    /// <param name="specification">The specification to be combined with the current one.</param>
    /// <returns>A new specification representing the logical OR operation.</returns>
    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    /// <summary>
    /// Creates a new specification that represents the logical NOT operation of the current specification.
    /// </summary>
    /// <returns>A new specification representing the logical NOT operation.</returns>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}