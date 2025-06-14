namespace Transponder.Core.Types;

public class AuditableEntity<TId> : Entity<TId>, IAuditableEntity where TId : struct
{
    protected AuditableEntity(TId id) : base(id)
    {
        var now = DateTime.UtcNow;
        
        CreatedDateUtcAt = DateOnly.FromDateTime(now);
        CreatedTimeUtcAt = TimeOnly.FromDateTime(now);
    }
    
    public DateOnly CreatedDateUtcAt { get; init; }
    public TimeOnly CreatedTimeUtcAt { get; init; }
    
    public DateOnly? UpdatedDateUtcAt { get; private set; }
    public TimeOnly? UpdatedTimeUtcAt { get; private set; }

    public DateOnly? DeletedDateUtcAt { get; private set; }
    public TimeOnly? DeletedTimeUtcAt { get; private set; }

    public void Update()
    {
        var now = DateTime.UtcNow;
        UpdatedDateUtcAt = DateOnly.FromDateTime(now);
        UpdatedTimeUtcAt = TimeOnly.FromDateTime(now);
    }

    protected void Delete()
    {
        var now = DateTime.UtcNow;
        DeletedDateUtcAt = DateOnly.FromDateTime(now);
        DeletedTimeUtcAt = TimeOnly.FromDateTime(now);
    }
}