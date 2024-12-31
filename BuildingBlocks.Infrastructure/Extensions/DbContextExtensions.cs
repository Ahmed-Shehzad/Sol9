using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbContext"/> to retrieve default schema names.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Gets the default schema name for a given type derived from <see cref="DbContext"/>.
    /// </summary>
    /// <param name="type">The type derived from <see cref="DbContext"/>.</param>
    /// <returns>The default schema name, or <c>null</c> if not found.</returns>
    private static string? GetDefaultSchema(MemberInfo type)
    {
        return type.Name.Replace(nameof(DbContext), string.Empty).ToFirstLetterLowerCase();
    }

    /// <summary>
    /// Gets the default schema name for a given type derived from <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The type derived from <see cref="DbContext"/>.</typeparam>
    /// <returns>The default schema name, or <c>null</c> if not found.</returns>
    public static string? GetDefaultSchema<TContext>() where TContext : DbContext
    {
        return GetDefaultSchema(typeof(TContext));
    }

    /// <summary>
    /// Gets the default schema name for the given <see cref="DbContext"/> instance.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> instance.</param>
    /// <returns>The default schema name, or <c>null</c> if not found.</returns>
    public static string? GetDefaultSchema(this DbContext context)
    {
        return GetDefaultSchema(context.GetType());
    }
}