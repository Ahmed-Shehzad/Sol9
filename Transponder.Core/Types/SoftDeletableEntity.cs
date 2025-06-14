namespace Transponder.Core.Types;

public class SoftDeletableEntity<TId> : AuditableEntity<TId>, ISoftDeletableEntity where TId : struct
{
    protected SoftDeletableEntity(TId id) : base(id)
    {

    }
    
    public bool IsDeleted { get; private set; }

    public new void Delete()
    {
        IsDeleted = true;
        base.Delete();
    }
}