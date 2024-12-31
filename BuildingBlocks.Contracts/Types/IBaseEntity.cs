namespace BuildingBlocks.Contracts.Types;

public interface IBaseEntity
{
    public Ulid Id { get; protected init; }
}