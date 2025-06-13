namespace Transponder.Core.Types;

public interface ISoftDeletableEntity
{
    bool IsDeleted { get; }
}