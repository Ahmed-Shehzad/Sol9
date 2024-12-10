using System.ComponentModel;
using System.Reflection;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Extensions.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Utilities.Types;

/// <summary>
/// Represents an accessor for identity information.
/// </summary>
/// <typeparam name="TIdentity">The type of the identity.</typeparam>
public sealed class IdentityAccessor<TIdentity> : IIdentityAccessor<TIdentity>
{
    private static readonly AsyncLocal<TIdentity?> _identity = new();

    /// <summary>
    /// Gets or sets the current identity.
    /// </summary>
    public TIdentity? Identity
    {
        get => _identity.Value; 
        set => _identity.Value = value;
    }
        
    /// <summary>
    /// Gets a value indicating whether the current identity is authenticated.
    /// </summary>
    public bool IsAuthenticated
    {
        get => _identity.Value != null;
    }

    /// <summary>
    /// Gets or sets the current identity as an object.
    /// </summary>
    object? IIdentityAccessor.Identity
    {
        get => Identity;
        set => Identity = (TIdentity?)value;
    }
        
    /// <summary>
    /// Gets the type of the identity.
    /// </summary>
    Type IIdentityAccessor.IdentityType
    {
        get => typeof(TIdentity);
    }
}

/// <summary>
/// Extension methods for configuring identity provider from claims in an ASP.NET Core application.
/// </summary>
    public static class ApplicationBuilderExtensions
    {
        private static List<(PropertyInfo p, FromClaimsAttribute?)>? _reflectionPropertyCache;

        private static Dictionary<string, string> AliasMap { get; } = new()
        {
            {"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "email"},
            {"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "family_name"},
            {"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "given_name"},
            {"http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "role"},
        };

        /// <summary>
        /// Adds middleware to the application's request pipeline to populate the identity from claims.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseIdentityProviderFromClaims(this IApplicationBuilder builder)
        {
            builder.Use((httpContext, next) =>
            {
                var services = httpContext.RequestServices;
                var accessor = services.GetRequiredService<IIdentityAccessor>();
                accessor.Identity = CreateIdentity( services.GetRequiredService<IHttpContextAccessor>(), accessor.IdentityType);
                return next();
            });
            return builder;
        }

        /// <summary>
        /// Creates an instance of the specified identity type using claims from the HTTP context.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="type">The type of the identity.</param>
        /// <returns>An instance of the identity type, or null if the user is not authenticated.</returns>
        private static object? CreateIdentity(IHttpContextAccessor httpContextAccessor, Type type)
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity is not { IsAuthenticated: true }) return null;
            
            _reflectionPropertyCache ??= type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => (p, p.GetCustomAttribute<FromClaimsAttribute>()))
                .Where(p => p.Item2 != null)
                .ToList();
            
            var claims = user.Claims
                .Select(c => (Type: AliasMap.TryGetValue(c.Type, out var value) ? value : c.Type, c.Value))
                .GroupBy(c => c.Type).ToDictionary(c => c.Key, c => c.Select(v => v.Value));
            var identity = Activator.CreateInstance(type);
            foreach (var (propertyInfo, attribute) in _reflectionPropertyCache)
            {
                if (attribute?.ClaimType != null && !claims.ContainsKey(attribute.ClaimType)) continue;
                if (attribute == null) continue;
                
                var propertyType = propertyInfo.PropertyType;
                
                var claimValues = claims[attribute.ClaimType];
                if (propertyType.IsArray)
                {
                    var innerType = propertyType.GetElementType();
                    if (innerType != typeof(string))
                    {
                        var typeConverter = TypeDescriptor.GetConverter(innerType!);
                        propertyInfo.SetValue(identity, claimValues.Select(value => typeConverter.ConvertFrom(value)).ToArray(), null);
                        continue;
                    }
                    propertyInfo.SetValue(identity, claimValues.ToArray(), null);
                    continue;
                }
                if (propertyType.IsEnumerable())
                {
                    var innerType = propertyType.GenericTypeArguments.First();
                    if (innerType != typeof(string))
                    {
                        var typeConverter = TypeDescriptor.GetConverter(innerType);
                        propertyInfo.SetValue(identity, claimValues.Select(value => typeConverter.ConvertFrom(value)).ToList(), null);
                        continue;
                    }
                    propertyInfo.SetValue(identity, claimValues.ToList(), null);
                    continue;
                }
                if (propertyType != typeof(string))
                {
                    var typeConverter = TypeDescriptor.GetConverter(propertyType);
                    propertyInfo.SetValue(identity,
                        claimValues.Select(value => typeConverter.ConvertFrom(value)).Single(), null);
                    continue;
                }
                propertyInfo.SetValue(identity, claimValues.Single(), null);
            }

            return identity;
        }
    }