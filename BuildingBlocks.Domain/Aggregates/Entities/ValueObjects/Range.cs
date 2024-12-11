namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a range of values from a start (From) to an end (To).
/// </summary>
/// <typeparam name="T">The type of values in the range.</typeparam>
public record Range<T>
{
    /// <summary>
    /// Represents a range of values from a start (from) to an end (to).
    /// </summary>
    /// <typeparam name="T">The type of values in the range.</typeparam>
    public Range(T from, T to)
    {
        From = from;
        To = to;
    }
    
    /// <summary>
    /// Gets the start value of the range.
    /// </summary>
    public T From { get; init; }

    /// <summary>
    /// Gets the end value of the range.
    /// </summary>
    public T To { get; init; }
}