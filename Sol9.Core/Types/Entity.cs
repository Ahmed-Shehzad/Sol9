namespace Sol9.Core.Types;

public abstract class Entity<TId> : IEntity<TId> where TId : struct
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; init; }
}