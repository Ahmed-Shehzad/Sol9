namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a geographical location with longitude and latitude coordinates.
/// </summary>
/// <param name="Longitude">The longitude coordinate of the geographical location.</param>
/// <param name="Latitude">The latitude coordinate of the geographical location.</param>
public record Coordinates(decimal Longitude, decimal Latitude)
{
    /// <summary>The longitude coordinate of the geographical location.</summary>
    public decimal Longitude { get; init; } = Longitude;

    /// <summary>The latitude coordinate of the geographical location.</summary>
    public decimal Latitude { get; init; } = Latitude;
}