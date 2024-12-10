namespace BuildingBlocks.Utilities.Types;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
/// <summary>
/// Represents an attribute that maps a property or field to a claim in a security principal.
/// </summary>
/// <param name="claimType">The type of claim to map to the property or field.</param>
public class FromClaimsAttribute(string claimType) : Attribute
{
    /// <summary>
    /// Gets or sets the type of claim to map to the property or field.
    /// </summary>
    public string ClaimType { get; set; } = claimType;
}