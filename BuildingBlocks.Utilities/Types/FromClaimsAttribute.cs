namespace BuildingBlocks.Utilities.Types;

/// <summary>
/// Represents an attribute that can be applied to properties or fields in a class to specify the claim type from which the value should be mapped.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class FromClaimsAttribute(string claimType) : Attribute
{
    /// <summary>
    /// Gets or sets the type of claim to map to the property or field.
    /// </summary>
    public string ClaimType { get; set; } = claimType;
}