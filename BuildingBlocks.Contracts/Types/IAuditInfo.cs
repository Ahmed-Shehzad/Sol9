namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines the properties for auditing information such as creation, update, and deletion.
/// </summary>
public interface IAuditInfo
{
    /// <summary>
    /// Gets or sets the date when the entity was created.
    /// </summary>
    DateOnly? CreatedDateAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the entity was created.
    /// </summary>
    TimeOnly? CreatedTimeAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date when the entity was last updated.
    /// </summary>
    DateOnly? UpdatedDateAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the entity was last updated.
    /// </summary>
    TimeOnly? UpdatedTimeAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date when the entity was deleted.
    /// </summary>
    DateOnly? DeletedDateAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the entity was deleted.
    /// </summary>
    TimeOnly? DeletedTimeAt { get; set; }

    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    string? DeletedBy { get; set; }
}