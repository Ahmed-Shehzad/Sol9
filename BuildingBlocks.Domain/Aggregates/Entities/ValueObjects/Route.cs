namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a route with a list of waypoints, estimated time, and distance in kilometers.
/// </summary>
/// <param name="Waypoints">A list of coordinates representing the waypoints of the route.</param>
/// <param name="EstimatedTime">The estimated time to complete the route.</param>
/// <param name="DistanceInKm">The distance of the route in kilometers.</param>
public record Route(List<Coordinates> Waypoints, TimeSpan EstimatedTime, decimal DistanceInKm)
{
    /// <summary>
    /// Gets the list of coordinates representing the waypoints of the route.
    /// </summary>
    public List<Coordinates> Waypoints { get; init; } = Waypoints;

    /// <summary>
    /// Gets the estimated time to complete the route.
    /// </summary>
    public TimeSpan EstimatedTime { get; init; } = EstimatedTime;

    /// <summary>
    /// Gets the distance of the route in kilometers.
    /// </summary>
    public decimal DistanceInKm { get; init; } = DistanceInKm;
}