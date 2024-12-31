namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a value with an associated unit.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public record UnitValue<T>
{
    /// <summary>
    /// Represents a value with an associated unit.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="unit">The unit of the value.</param>
    private UnitValue(T value, string unit)
    {
        Value = value;
        Unit = unit;
    }
    public static UnitValue<T> Create(T value, string unit)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        
        return new UnitValue<T>(value, unit);
    }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value { get; init; }

    /// <summary>
    /// Gets or sets the unit of the value.
    /// </summary>
    /// <value>The unit of the value. Can be null if no unit is specified.</value>
    public string Unit { get; init; }
}