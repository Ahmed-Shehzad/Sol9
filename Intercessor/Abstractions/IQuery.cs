namespace Intercessor.Abstractions;

/// <summary>
/// Marker interface to represent a query that produces a response when handled.
/// </summary>
public interface IQuery : IRequest;
    
/// <summary>
/// Represents a query that produces a response when handled by <see cref="IRequestHandler{TRequest, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">
/// The type of response that the request will produce when handled.
/// </typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>, IQuery;
    
/// <summary>
/// Represents a cached query that produces a response when handled by <see cref="IRequestHandler{TRequest, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">
/// The type of response that the request will produce when handled.
/// </typeparam>
public interface ICachedQuery<TResponse> : IQuery<TResponse>
{
    /// <summary>
    /// The cache key associated with the request.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// The duration for which the cache is valid. If null, the cache has no expiration.
    /// </summary>
    TimeSpan? CacheDuration { get; }
}