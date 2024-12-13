using BuildingBlocks.Contracts.Types;
using Orders.Domain.Aggregates;
using Orders.Domain.Aggregates.Entities;

namespace Orders.Infrastructure.Contexts.Contracts;

/// <summary>
/// Defines the read-only interface for the Orders database context.
/// </summary>
public interface IOrdersReadOnlyDbContext : IReadOnlyDbContext
{
    /// <summary>
    /// Gets a queryable collection of <see cref="Order"/> entities.
    /// </summary>
    /// <remarks>
    /// This property provides access to all Order entities stored in the database.
    /// It allows for querying and filtering operations without modifying the data.
    /// </remarks>
    /// <returns>An <see cref="IQueryable{Order}"/> representing the collection of Order entities.</returns>
    public IQueryable<Order> Orders { get; }
}