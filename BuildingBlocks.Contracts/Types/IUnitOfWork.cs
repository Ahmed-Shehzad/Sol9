namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents a unit of work for managing database transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of objects written to the database.</returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
