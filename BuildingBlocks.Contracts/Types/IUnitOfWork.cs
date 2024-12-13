namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents a unit of work for managing database transactions.
/// </summary>
public interface IUnitOfWork : IUnitOfWorkContext
{
    /// <summary>
    /// Executes the given operation within a database transaction and commits it.
    /// </summary>
    /// <param name="operation">The operation to be executed within the transaction.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitAsync(Func<IUnitOfWorkContext, Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the given operation within a database transaction, commits it, and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="operation">The operation to be executed within the transaction.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation, returning the result of type T.</returns>
    Task<T> CommitAsync<T>(Func<IUnitOfWorkContext, Task<T>> operation, CancellationToken cancellationToken = default);
}