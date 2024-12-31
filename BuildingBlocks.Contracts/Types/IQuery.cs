using MediatR;

namespace BuildingBlocks.Contracts.Types;

/// <summary>Represents a query in the application.</summary>
public interface IQuery;

/// <summary>
/// Represents a query that returns a result of type T.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
public interface IQuery<out T> : IRequest<T>, IQuery;
