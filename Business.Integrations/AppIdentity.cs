using BuildingBlocks.Utilities.Types;
using IdentityModel;

namespace Business.Integrations;

/// <summary>
/// Represents an application identity, containing user and tenant information extracted from JWT claims.
/// </summary>
public class AppIdentity
{
    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    [FromClaims(JwtClaimTypes.Name)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [FromClaims(JwtClaimTypes.FamilyName)]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [FromClaims(JwtClaimTypes.GivenName)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's unique identifier.
    /// </summary>
    [FromClaims(JwtClaimTypes.Id)]
    public Ulid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant's unique identifier.
    /// </summary>
    [FromClaims(CustomClaimTypes.TenantId)]
    public Ulid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the user's roles.
    /// </summary>
    [FromClaims(JwtClaimTypes.Role)]
    public string[] Roles { get; set; } = [];
}