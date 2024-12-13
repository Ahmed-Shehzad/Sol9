using BuildingBlocks.Contracts.Types;

namespace BuildingBlocks.Domain.Aggregates.Entities;

/// <summary>
/// Represents a base entity with audit information.
/// </summary>
/// <param name="id">The unique identifier for the entity.</param>
public abstract class BaseEntity(Ulid id) : IBaseEntity, IAuditInfo
{
    private static readonly DateTime TimeStampUtc = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Ulid Id { get; init; } = id;

    #region AuditInfo

    /// <summary>
    /// Gets or sets the date when the entity was created.
    /// </summary>
    public DateOnly? CreatedDateUtcAt { get; set; } = DateOnly.FromDateTime(TimeStampUtc);

    /// <summary>
    /// Gets or sets the time when the entity was created.
    /// </summary>
    public TimeOnly? CreatedTimeUtcAt { get; set; } = TimeOnly.FromDateTime(TimeStampUtc);

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; } = null;

    /// <summary>
    /// Gets or sets the date when the entity was last updated.
    /// </summary>
    public DateOnly? UpdatedDateUtcAt { get; set; } = null;

    /// <summary>
    /// Gets or sets the time when the entity was last updated.
    /// </summary>
    public TimeOnly? UpdatedTimeUtcAt { get; set; } = null;

    /// <summary>
    /// Gets or sets the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date when the entity was deleted.
    /// </summary>
    public DateOnly? DeletedDateUtcAt { get; set; } = null;

    /// <summary>
    /// Gets or sets the time when the entity was deleted.
    /// </summary>
    public TimeOnly? DeletedTimeUtcAt { get; set; } = null;

    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    public string? DeletedBy { get; set; } = null;

    #endregion AuditInfo
}