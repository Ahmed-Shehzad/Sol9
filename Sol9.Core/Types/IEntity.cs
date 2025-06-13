namespace Sol9.Core;

public interface IEntity<TId> where TId : struct
{
    public TId Id { get; init; }
}