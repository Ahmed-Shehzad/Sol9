namespace Sol9.Common;

public interface IEntity<TId> where TId : struct
{
    public TId Id { get; init; }
}