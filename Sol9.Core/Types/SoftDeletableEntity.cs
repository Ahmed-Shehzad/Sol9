namespace Sol9.Core.Types;

public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>, ISoftDeletableEntity where TId : struct
{
    protected SoftDeletableEntity(TId id) : base(id)
    {

    }
    
    public bool IsDeleted { get; private set; }

    protected override void Delete()
    {
        IsDeleted = true;
        base.Delete();
    }
}