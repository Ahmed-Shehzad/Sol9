using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Entities.ValueObjects;

public record OrderTimeFrame : Range<TimeOnly>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderTimeFrame"/> class.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday).</param>
    /// <param name="from">The start time of the operating hour.</param>
    /// <param name="to">The end time of the operating hour.</param>
    private OrderTimeFrame(int dayOfWeek, TimeOnly from, TimeOnly to) : base(from, to)
    {
        DayOfWeek = dayOfWeek;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="OrderTimeFrame"/> class.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week (0 = Sunday, 1 = Monday, ..., 6 = Saturday).</param>
    /// <param name="from">The start time of the operating hour.</param>
    /// <param name="to">The end time of the operating hour.</param>
    /// <returns>A new instance of the <see cref="OrderTimeFrame"/> class.</returns>
    public static OrderTimeFrame Create(int dayOfWeek, TimeOnly from, TimeOnly to)
    {
        return new OrderTimeFrame(dayOfWeek, from, to);
    }

    /// <summary>
    /// Gets the day of the week for this order time frame.
    /// </summary>
    public int DayOfWeek { get; init; }
}