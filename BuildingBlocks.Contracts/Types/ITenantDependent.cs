namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents an interface for entities that are tenant-dependent.
/// </summary>
public interface ITenantDependent
{
    /// <summary>
    /// Get the unique identifier of the tenant associated with the entity.
    /// </summary>
    /// <value>
    /// The unique identifier of the tenant. If the entity is not tenant-dependent, this value should be null.
    /// </value>
    Ulid? TenantId { get; }
}