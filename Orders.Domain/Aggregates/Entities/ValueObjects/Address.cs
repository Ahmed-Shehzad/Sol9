using NetTopologySuite.Geometries;

namespace Orders.Domain.Aggregates.Entities.ValueObjects
{
    public record Address(
        Point? Geography,
        string Street,
        string Number,
        string ZipCode,
        string City,
        string Country,
        string? Phone,
        string StateOrProvince,
        string? Company,
        string Name,
        string? Addition);
}