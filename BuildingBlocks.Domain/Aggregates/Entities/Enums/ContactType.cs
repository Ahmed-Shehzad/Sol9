using BuildingBlocks.Domain.Types;

namespace BuildingBlocks.Domain.Aggregates.Entities.Enums;

public class ContactType : Enumeration
{
    private ContactType(int id, string value) : base(id, value)
    {
    }
    
    public static ContactType Landline { get; } = new(1, nameof(Landline));
    public static ContactType Work { get; } = new(2, nameof(Work));
    public static ContactType Mobile { get; } = new(3, nameof(Mobile));
    public static ContactType Fax { get; } = new(4, nameof(Fax));
}