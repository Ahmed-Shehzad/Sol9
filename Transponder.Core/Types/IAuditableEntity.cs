namespace Transponder.Core.Types;

public interface IAuditableEntity
{
    DateOnly CreatedDateUtcAt { get; }
    TimeOnly CreatedTimeUtcAt { get; }

    DateOnly? UpdatedDateUtcAt { get; }
    TimeOnly? UpdatedTimeUtcAt { get; }

    DateOnly? DeletedDateUtcAt { get; }
    TimeOnly? DeletedTimeUtcAt { get; }
}