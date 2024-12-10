using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;
using NetTopologySuite.Geometries;

namespace BuildingBlocks.Infrastructure.Types;

public class GeographyUtils
{
    // See https://epsg.io/4326
    private const int Srid = 4326;
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
        
    public static Geography? FromPoint(Point? point)
    {
        return point == null ? null : new Geography(Convert.ToDecimal(point.X), Convert.ToDecimal(point.Y));
    }
}