namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines the properties for auditing information such as creation, update, and deletion.
/// </summary>
public interface IAuditInfo
{
    /// <summary>
    /// Gets or sets the date when the entity was created.
    /// </summary>
    DateOnly? CreatedDateUtcAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the entity was created.
    /// </summary>
    TimeOnly? CreatedTimeUtcAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date when the entity was last updated.
    /// </summary>
    DateOnly? UpdatedDateUtcAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the entity was last updated.
    /// </summary>
    TimeOnly? UpdatedTimeUtcAt { get; set; }

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
    DateOnly? DeletedDateUtcAt { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the entity was deleted.
    /// </summary>
    TimeOnly? DeletedTimeUtcAt { get; set; }

    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    string? DeletedBy { get; set; }
}