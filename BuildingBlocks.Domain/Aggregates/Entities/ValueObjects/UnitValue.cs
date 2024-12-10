namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a value with an associated unit.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public record UnitValue<T>
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value { get; init; }

    /// <summary>
    /// Gets or sets the unit of the value.
    /// </summary>
    /// <value>The unit of the value. Can be null if no unit is specified.</value>
    public string? Unit { get; init; }
}