using System.Linq.Expressions;

namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines a contract for a repository that provides CRUD operations and asynchronous methods for a specific model type.
/// </summary>
/// <typeparam name="TModel">The type of model this repository operates on.</typeparam>
public interface IRepository<TModel> : IDisposable where TModel : class
{
    /// <summary>
    /// Finds all models in the repository that satisfy the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous task that represents the operation. The task result contains a collection of models that satisfy the predicate.</returns>
    Task<ICollection<TModel>> FindAllByAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all models in the repository.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous task that represents the operation. The task result contains a collection of all models in the repository.</returns>
    Task<ICollection<TModel>> FindAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single model in the repository that satisfies the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous task that represents the operation. The task result contains the first model that satisfies the predicate, or null if no such model is found.</returns>
    Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any model in the repository satisfies the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous task that represents the operation. The task result contains true if any model satisfies the predicate; otherwise, false.</returns>
    Task<bool> AnyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new model to the repository.
    /// </summary>
    /// <param name="entity">The model to add.</param>
    void Add(TModel entity);

    /// <summary>
    /// Adds a range of new models to the repository.
    /// </summary>
    /// <param name="entities">The models to add.</param>
    void AddRange(IEnumerable<TModel> entities);

    /// <summary>
    /// Updates an existing model in the repository.
    /// </summary>
    /// <param name="entity">The model to update.</param>
    void Update(TModel entity);

    /// <summary>
    /// Updates a range of existing models in the repository.
    /// </summary>
    /// <param name="entities">The models to update.</param>
    void UpdateRange(IEnumerable<TModel> entities);

    /// <summary>
    /// Removes a model from the repository.
    /// </summary>
    /// <param name="entity">The model to remove.</param>
    void Remove(TModel entity);

    /// <summary>
    /// Removes a range of models from the repository.
    /// </summary>
    /// <param name="entities">The models to remove.</param>
    void RemoveRange(IEnumerable<TModel> entities);
}