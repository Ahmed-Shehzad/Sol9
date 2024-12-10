using NetTopologySuite.Geometries;

namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents an address with geographical and location details.
/// </summary>
public record Address
{
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
    private Address(Point? geography, string street, string number, string zipCode, string city, string state, string country)
    {
        Geography = geography;
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
    public static Address Create(Point? geography, string street, string number, string zipCode, string city, string state, string country)
    {
        return new Address(geography, street, number, zipCode, city, state, country);
    }

    /// <summary>
    /// Gets the geographical point representing the address location.
    /// </summary>
    public Point? Geography { get; init; }

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