// using BuildingBlocks.Contracts.Types;
//
// namespace BuildingBlocks.Utilities.Types;
//
// /// <summary>
// /// Represents an accessor for identity information.
// // /// </summary>
// /// <typeparam name="TIdentity">The type of the identity.</typeparam>
// public sealed class IdentityAccessor<TIdentity> : IIdentityAccessor<TIdentity>
// {
//     private static readonly AsyncLocal<TIdentity?> _identity = new();
//
//     /// <summary>
//     /// Gets or sets the current identity.
//     /// </summary>
//     public TIdentity? Identity
//     {
//         get => _identity.Value;
//         set => _identity.Value = value;
//     }
//
//     /// <summary>
//     /// Gets a value indicating whether the current identity is authenticated.
//     /// </summary>
//     public bool IsAuthenticated
//     {
//         get => _identity.Value != null;
//     }
//
//     /// <summary>
//     /// Gets or sets the current identity as an object.
//     /// </summary>
//     object? IIdentityAccessor.Identity
//     {
//         get => Identity;
//         set => Identity = (TIdentity?)value;
//     }
//
//     /// <summary>
//     /// Gets the type of the identity.
//     /// </summary>
//     Type IIdentityAccessor.IdentityType
//     {
//         get => typeof(TIdentity);
//     }
// }

