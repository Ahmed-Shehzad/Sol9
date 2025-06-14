namespace Sol9.Core.Types;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; }
}