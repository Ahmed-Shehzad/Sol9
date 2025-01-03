﻿using BuildingBlocks.Utilities.Types;

namespace BuildingBlocks.Domain.Aggregates.Entities.Enums;

/// <summary>
/// Represents different types of contact information.
/// </summary>
public class ContactType : Enumeration
{
    public ContactType() : base()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ContactType"/> class.
    /// </summary>
    /// <param name="key">The unique identifier of the contact type.</param>
    /// <param name="value">The display name of the contact type.</param>
    private ContactType(int key, string value) : base(key, value)
    {
    }
    
    /// <summary>
    /// Represents a landline contact type.
    /// </summary>
    public static ContactType Landline { get; } = new(1, nameof(Landline));
    
    /// <summary>
    /// Represents a work contact type.
    /// </summary>
    public static ContactType Work { get; } = new(2, nameof(Work));
    
    /// <summary>
    /// Represents a mobile contact type.
    /// </summary>
    public static ContactType Mobile { get; } = new(3, nameof(Mobile));
    
    /// <summary>
    /// Represents a fax contact type.
    /// </summary>
    public static ContactType Fax { get; } = new(4, nameof(Fax));
}