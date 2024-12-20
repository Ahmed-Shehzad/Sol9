using NetTopologySuite.Geometries;
using Coordinates = BuildingBlocks.Domain.Aggregates.Entities.ValueObjects.Coordinates;

namespace BuildingBlocks.Domain.Aggregates.Utilities;

public static class GeographyUtils
{
    // See https://epsg.io/4326
    private const int Srid = 4326;

    /// <summary>
    /// Converts a Coordinates object to a Point object with the specified SRID.
    /// </summary>
    /// <param name="geometry">The Coordinates object to convert. If null, the function returns null.</param>
    /// <returns>A Point object representing the coordinates of the input Coordinates object. If the input Coordinates object has more or less than 2 coordinates, a NotSupportedException is thrown.</returns>
    public static Point? ToPoint(Coordinates? geometry)
    {
        if (geometry == null) return null;

        return new Point(
            Convert.ToDouble(decimal.Round(geometry.Longitude, 6, MidpointRounding.AwayFromZero)),
            Convert.ToDouble(decimal.Round(geometry.Latitude, 6, MidpointRounding.AwayFromZero)))
        {
            SRID = Srid
        };
    }

    /// <summary>
    /// Converts a Point object to a Coordinates object.
    /// </summary>
    /// <param name="point">The Point object to convert. If null, the function returns null.</param>
    /// <returns>A Coordinates object representing the coordinates of the input Point object.</returns>
    public static Coordinates? FromPoint(Point? point)
    {
        return point == null ? null : new Coordinates(Convert.ToDecimal(point.X), Convert.ToDecimal(point.Y));
    }
}