namespace BuildingBlocks.Contracts.Types;

/// <summary>
/// Defines a base interface for accessing identity information.
/// </summary>
public interface IIdentityAccessor
{
    Type IdentityType { get; }
    object? Identity { get; set; }
    bool IsAuthenticated { get; }
}

/// <summary>
/// Provides a generic interface for accessing and managing identity information.
/// </summary>
/// <typeparam name="TIdentity">The type of the identity object.</typeparam>
public interface IIdentityAccessor<TIdentity> : IIdentityAccessor
{
    /// <summary>
    /// Gets or sets the strongly-typed identity object.
    /// </summary>
    new TIdentity? Identity { get; set; }
}