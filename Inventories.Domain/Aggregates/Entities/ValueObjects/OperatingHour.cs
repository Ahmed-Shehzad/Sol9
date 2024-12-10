using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Inventories.Domain.Aggregates.Entities.ValueObjects;

public record OperatingHour : Range<TimeOnly>
{
    private OperatingHour(int dayOfWeek, TimeOnly from, TimeOnly to) : base(from, to)
    {
        DayOfWeek = dayOfWeek;
    }
    public static OperatingHour Create(int dayOfWeek, TimeOnly from, TimeOnly to)
    {
        return new OperatingHour(dayOfWeek, from, to);
    }

    public int DayOfWeek { get; init; }
}