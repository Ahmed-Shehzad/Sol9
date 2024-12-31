// using BuildingBlocks.Contracts.Types;
//
// namespace BuildingBlocks.Utilities.Types;
//
// /// <summary>
// /// Represents a static identity accessor that provides access to a static identity of type T.
// /// </summary>
// /// <typeparam name="T">The type of the identity.</typeparam>
// public class StaticIdentityAccessor<T>(T? identity) : IIdentityAccessor<T>
// {
//     /// <summary>
//     /// Gets the type of the identity.
//     /// </summary>
//     public Type IdentityType
//     {
//         get => typeof(T);
//     }
//
//     /// <summary>
//     /// Gets the static identity.
//     /// Setting this property throws a NotSupportedException as the identity is static.
//     /// </summary>
//     public T? Identity { get => identity; set => throw new NotSupportedException(); }
//
//     /// <summary>
//     /// Gets or sets the static identity.
//     /// Setting this property throws a NotSupportedException as the identity is static.
//     /// </summary>
//     object? IIdentityAccessor.Identity
//     {
//         get => Identity;
//         set => Identity = (T?)value;
//     }
//
//     /// <summary>
//     /// Gets a value indicating whether the identity is authenticated.
//     /// In this case, it always returns true as the identity is static.
//     /// </summary>
//     public bool IsAuthenticated
//     {
//         get => true;
//     }
// }