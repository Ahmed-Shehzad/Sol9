using BuildingBlocks.Utilities.Types;

namespace Locations.Service.Osrm.Enumerations;

/// <summary>
/// Represents a geographical format enumeration.
/// </summary>
public class GeoFormat : Enumeration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeoFormat"/> class.
    /// </summary>
    public GeoFormat() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoFormat"/> class with a specified key and value.
    /// </summary>
    /// <param name="key">The key of the enumeration.</param>
    /// <param name="value">The value of the enumeration.</param>
    private GeoFormat(int key, string value) : base(key, value)
    {
    }

    /// <summary>
    /// Represents the Polyline format.
    /// </summary>
    public static readonly GeoFormat Polyline = new(1, nameof(Polyline));

    /// <summary>
    /// Represents the Polyline6 format.
    /// </summary>
    public static readonly GeoFormat Polyline6 = new(2, nameof(Polyline6));

    /// <summary>
    /// Represents the GeoJson format.
    /// </summary>
    public static readonly GeoFormat GeoJson = new(3, nameof(GeoJson));
}