using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Inventories.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents an operating hour for a specific day of the week.
/// </summary>
public record OperatingHour : Range<TimeOnly>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperatingHour"/> class.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday).</param>
    /// <param name="from">The start time of the operating hour.</param>
    /// <param name="to">The end time of the operating hour.</param>
    private OperatingHour(DayOfWeek dayOfWeek, TimeOnly from, TimeOnly to) : base(from, to)
    {
        DayOfWeek = dayOfWeek;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="OperatingHour"/> class.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday).</param>
    /// <param name="from">The start time of the operating hour.</param>
    /// <param name="to">The end time of the operating hour.</param>
    /// <returns>A new instance of the <see cref="OperatingHour"/> class.</returns>
    public static OperatingHour Create(DayOfWeek dayOfWeek, TimeOnly from, TimeOnly to)
    {
        return new OperatingHour(dayOfWeek, from, to);
    }

    /// <summary>
    /// Gets the day of the week for this operating hour.
    /// </summary>
    public DayOfWeek DayOfWeek { get; init; }
}