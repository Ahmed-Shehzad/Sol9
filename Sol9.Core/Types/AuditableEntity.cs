namespace Sol9.Core.Types;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity where TId : struct
{
    protected AuditableEntity(TId id) : base(id)
    {

    }
    
    public DateOnly CreatedDateUtcAt { get; init; }
    public TimeOnly CreatedTimeUtcAt { get; init; }
    
    public DateOnly? UpdatedDateUtcAt { get; private set; }
    public TimeOnly? UpdatedTimeUtcAt { get; private set; }

    public DateOnly? DeletedDateUtcAt { get; private set; }
    public TimeOnly? DeletedTimeUtcAt { get; private set; }
    
    protected virtual void Update()
    {
        var now = DateTime.UtcNow;
        UpdatedDateUtcAt = DateOnly.FromDateTime(now);
        UpdatedTimeUtcAt = TimeOnly.FromDateTime(now);
    }

    protected virtual void Delete()
    {
        var now = DateTime.UtcNow;
        DeletedDateUtcAt = DateOnly.FromDateTime(now);
        DeletedTimeUtcAt = TimeOnly.FromDateTime(now);
    }
}