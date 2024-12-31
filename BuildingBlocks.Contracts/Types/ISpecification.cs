using System.Linq.Expressions;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents a specification for filtering and querying objects of type T.
/// </summary>
/// <typeparam name="T">The type of objects to be filtered.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Converts the specification into a LINQ expression that can be used for filtering.
    /// </summary>
    /// <returns>An Expression representing the specification.</returns>
    Expression<Func<T, bool>> ToExpression();
}