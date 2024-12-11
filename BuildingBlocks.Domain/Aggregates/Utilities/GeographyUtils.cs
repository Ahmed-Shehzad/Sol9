using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using NetTopologySuite.Geometries;

namespace BuildingBlocks.Domain.Aggregates.Utilities;

public static class GeographyUtils
{
    // See https://epsg.io/4326
    private const int Srid = 4326;

    /// <summary>
    /// Converts a Geography object to a Point object with the specified SRID.
    /// </summary>
    /// <param name="geometry">The Geography object to convert. If null, the function returns null.</param>
    /// <returns>A Point object representing the coordinates of the input Geography object. If the input Geography object has more or less than 2 coordinates, a NotSupportedException is thrown.</returns>
    public static Point? ToPoint(Geography? geometry)
    {
        if (geometry == null) return null;
        if (geometry.Coordinates.Length != 2) throw new NotSupportedException("GEOJSON coordinate field must have exact length of 2.");
        return new Point(
            Convert.ToDouble(decimal.Round(geometry.Coordinates[0], 6, MidpointRounding.AwayFromZero)), 
            Convert.ToDouble(decimal.Round(geometry.Coordinates[1], 6, MidpointRounding.AwayFromZero)))
        {
            SRID = Srid
        };
    }
        
    /// <summary>
    /// Converts a Point object to a Geography object.
    /// </summary>
    /// <param name="point">The Point object to convert. If null, the function returns null.</param>
    /// <returns>A Geography object representing the coordinates of the input Point object.</returns>
    public static Geography? FromPoint(Point? point)
    {
        return point == null ? null : new Geography(Convert.ToDecimal(point.X), Convert.ToDecimal(point.Y));
    }
}