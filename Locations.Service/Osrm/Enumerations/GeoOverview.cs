using BuildingBlocks.Utilities.Types;

namespace Locations.Service.Osrm.Enumerations;

/// <summary>
/// Represents a geographical overview enumeration.
/// </summary>
public class GeoOverview : Enumeration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeoOverview"/> class.
    /// </summary>
    public GeoOverview() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoOverview"/> class with a specified key and value.
    /// </summary>
    /// <param name="key">The key of the enumeration.</param>
    /// <param name="value">The value of the enumeration.</param>
    private GeoOverview(int key, string value) : base(key, value)
    {
    }

    /// <summary>
    /// Represents the Simplified overview.
    /// </summary>
    public static readonly GeoOverview Simplified = new(1, nameof(Simplified));

    /// <summary>
    /// Represents the Full overview.
    /// </summary>
    public static readonly GeoOverview Full = new(2, nameof(Full));

    /// <summary>
    /// Represents the False overview.
    /// </summary>
    public static readonly GeoOverview False = new(3, nameof(False));
}