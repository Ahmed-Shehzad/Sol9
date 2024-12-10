namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a geographical location with longitude and latitude coordinates.
/// </summary>
/// <param name="Longitude">The longitude coordinate of the geographical location.</param>
/// <param name="Latitude">The latitude coordinate of the geographical location.</param>
public record Geography(decimal Longitude, decimal Latitude)
{
    /// <summary>
    /// Gets the coordinates of the geographical location as an array of decimal values.
    /// The first element is the longitude and the second element is the latitude.
    /// </summary>
    public decimal[] Coordinates { get; init; } = [Longitude, Latitude];

    /// <summary>The longitude coordinate of the geographical location.</summary>
    public decimal Longitude { get; init; } = Longitude;

    /// <summary>The latitude coordinate of the geographical location.</summary>
    public decimal Latitude { get; init; } = Latitude;

}