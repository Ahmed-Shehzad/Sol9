using BuildingBlocks.Domain.Aggregates.Utilities;
using NetTopologySuite.Geometries;

namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents an address with geographical and location details.
/// </summary>
public record Address
{
    public Address()
    {
    }
    /// <summary>
    /// Represents an address with geographical and location details.
    /// </summary>
    /// <param name="geography">The geographical point representing the address location.</param>
    /// <param name="street">The street name of the address.</param>
    /// <param name="number">The street number or building number of the address.</param>
    /// <param name="zipCode">The postal or ZIP code of the address.</param>
    /// <param name="city">The city name of the address.</param>
    /// <param name="state">The state or province name of the address.</param>
    /// <param name="country">The country name of the address.</param>
    private Address(Geography? geography, string street, string number, string zipCode, string city, string state, string country)
    {
        Point = GeographyUtils.ToPoint(geography);
        Street = street;
        Number = number;
        ZipCode = zipCode;
        City = city;
        State = state;
        Country = country;
    }

    /// <summary>
    /// Represents an address with geographical and location details.
    /// </summary>
    /// <param name="geography">The geographical point representing the address location.</param>
    /// <param name="street">The street name of the address.</param>
    /// <param name="number">The street number or building number of the address.</param>
    /// <param name="zipCode">The postal or ZIP code of the address.</param>
    /// <param name="city">The city name of the address.</param>
    /// <param name="state">The state or province name of the address.</param>
    /// <param name="country">The country name of the address.</param>
    /// <returns>A new instance of the <see cref="Address"/> class.</returns>
    public static Address Create(Geography? geography, string street, string number, string zipCode, string city, string state, string country)
    {
        return new Address(geography, street, number, zipCode, city, state, country);
    }

    /// <summary>
    /// Gets or sets the geographical representing the address location.
    /// This property is marked with <see /> attribute to exclude it from database mapping.
    /// </summary>
    /// <remarks>
    /// The <see cref="Geography"/> property provides a convenient way to access and manipulate the geographical point using the <see cref="GeographyUtils"/> class.
    /// The getter retrieves the geography from the <see cref="Point"/> using <see cref="GeographyUtils.FromPoint(NetTopologySuite.Geometries.Point)"/> method.
    /// The setter updates the <see cref="Point"/> using <see cref="GeographyUtils.ToPoint(ValueObjects.Geography)"/> method.
    /// </remarks>
    public Geography? Geography
    {
        get => GeographyUtils.FromPoint(Point);
        init => Point = GeographyUtils.ToPoint(value);
    }

    /// <summary>
    /// Gets the geographical point representing the address location.
    /// </summary>
    public Point? Point { get; init; }

    /// <summary>
    /// Gets the street name of the address.
    /// </summary>
    public string Street { get; init; }

    /// <summary>
    /// Gets the street number or building number of the address.
    /// </summary>
    public string Number { get; init; }

    /// <summary>
    /// Gets the postal or ZIP code of the address.
    /// </summary>
    public string ZipCode { get; init; }

    /// <summary>
    /// Gets the city name of the address.
    /// </summary>
    public string City { get; init; }

    /// <summary>
    /// Gets the state or province name of the address.
    /// </summary>
    public string State { get; init; }

    /// <summary>
    /// Gets the country name of the address.
    /// </summary>
    public string Country { get; init; }
}