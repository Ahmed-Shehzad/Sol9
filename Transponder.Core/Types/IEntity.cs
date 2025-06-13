namespace Transponder.Core.Types;

public interface IEntity<TId> where TId : struct
{
    public TId Id { get; init; }
}