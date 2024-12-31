using MediatR;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines a handler for a specific query type that returns a response.
/// </summary>
/// <typeparam name="TQuery">The type of the query to be handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
/// <remarks>
/// This interface extends IRequestHandler from MediatR, allowing it to be used within
/// a mediator pattern for handling queries. It ensures that the handler can only process
/// queries that implement the IQuery interface with the corresponding response type.
/// </remarks>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> 
    where TQuery : IQuery<TResponse>;
