using BuildingBlocks.Domain.Aggregates.Entities.Enums;

namespace BuildingBlocks.Domain.Aggregates.Entities.ValueObjects;

/// <summary>
/// Represents a contact information in the system.
/// </summary>
public record Contact
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Contact"/> class.
    /// </summary>
    /// <param name="type">The type of the contact.</param>
    /// <param name="number">The contact number.</param>
    public Contact(ContactType type, string number)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        
        Type = type;
        Number = number;
    }
    
    /// <summary>
    /// Gets the type of the contact.
    /// </summary>
    /// <value>
    /// The type of the contact.
    /// </value>
    public ContactType Type { get; init; }

    /// <summary>
    /// Gets the contact number.
    /// </summary>
    /// <value>
    /// The contact number.
    /// </value>
    public string Number { get; init; }
}