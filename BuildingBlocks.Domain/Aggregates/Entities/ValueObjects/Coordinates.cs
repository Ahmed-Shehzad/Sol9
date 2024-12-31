using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a geographical location with longitude and latitude coordinates.
/// </summary>
public record Coordinates
{
    /// <summary>
    /// Represents a geographical location with longitude and latitude coordinates.
    /// </summary>
    /// <param name="longitude">The longitude coordinate of the geographical location.</param>
    /// <param name="latitude">The latitude coordinate of the geographical location.</param>
    private Coordinates(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
    public static Coordinates Create(double longitude, double latitude)
    {
        return new Coordinates(longitude, latitude);
    }

    /// <summary>The longitude coordinate of the geographical location.</summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    /// <summary>The latitude coordinate of the geographical location.</summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }
}