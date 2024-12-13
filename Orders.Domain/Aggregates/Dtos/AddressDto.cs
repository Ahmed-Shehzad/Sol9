using BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

namespace Orders.Domain.Aggregates.Dtos;

public record AddressDto
{
    private AddressDto(Geography? geography, string street, string number, string zipCode, string city, string state, string country)
    {
        Geography = geography;
        Street = street;
        Number = number;
        ZipCode = zipCode;
        City = city;
        State = state;
        Country = country;
    }
    public static AddressDto Create(Geography? geography, string street, string number, string zipCode, string city, string state,
        string country)
    {
        return new AddressDto(geography, street, number, zipCode, city, state, country);
    }

    /// <summary>
    /// Gets the geographical point representing the address location.
    /// </summary>
    public Geography? Geography { get; init; }

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