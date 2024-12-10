namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Represents an interface for entities that are dependent on a user.
/// </summary>
public interface IUserDependent
{
    /// <summary>
    /// Get the unique identifier of the user associated with this entity.
    /// </summary>
    /// <value>
    /// The unique identifier of the user. If the entity is not associated with a user, this value should be null.
    /// </value>
    Ulid? UserId { get; }
}