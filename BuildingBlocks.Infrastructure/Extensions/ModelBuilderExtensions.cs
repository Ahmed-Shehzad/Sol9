using System.Linq.Expressions;
using BuildingBlocks.Contracts.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IAuditInfo).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var propertyMethod = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
            
            if (propertyMethod is null) continue;
            
            var isDeletedProperty = Expression.Call(propertyMethod, parameter, Expression.Constant("IsDeleted"));
            var filter = Expression.Lambda(Expression.Not(isDeletedProperty), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
    
    public static void ApplyTenantQueryFilter<T>(this ModelBuilder modelBuilder, Ulid? tenantId) where T : class, ITenantDependent
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => tenantId.HasValue && e.TenantId.Equals(tenantId));
    }
    
    public static void ApplyUserQueryFilter<T>(this ModelBuilder modelBuilder, Ulid? userId) where T : class, IUserDependent
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => userId.HasValue && e.UserId.Equals(userId));
    }
    
    /// <summary>
    ///  Adds value converters for DateTime and Nullable&lt;DateTime&gt; for automatic UTC conversions.
    /// </summary>
    /// <param name="builder"></param>
    public static void ApplyDateTimeUtcConversion(this ModelBuilder builder)
    {
        builder.ApplyConfigurationForPropertyType(property =>
        {
            property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(
                v => v, 
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null));
        }, type => type == typeof(DateTime?));
        builder.ApplyConfigurationForPropertyType(property =>
        {
            property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                v => v, 
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        }, type => type == typeof(DateTime));
    }

    /// <summary>
    ///     Adds automatic enum to string conversion.
    /// </summary>
    /// <param name="builder"></param>
    public static void ApplyEnumToStringConversion(this ModelBuilder builder)
    {
        builder.ApplyConfigurationForPropertyType(property =>
        {
            var converter = (ValueConverter?)
                Activator.CreateInstance(typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType), [null]);
            property.SetValueConverter(converter);
            // property.SetValueComparer();
        }, type => type.IsEnum);
    }

    /// <summary>
    ///     Automatically apply value converter on a specific type.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="configure"></param>
    /// <param name="compare"></param>
    /// <returns></returns>
    private static ModelBuilder ApplyConfigurationForPropertyType(this ModelBuilder modelBuilder, Action<IMutableProperty> configure,
        Func<Type, bool> compare)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => compare(p.ClrType)))
            {
                configure(property);
            }
        }
        return modelBuilder;
    }

    public static void ConfigureDefaultSchema(this ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.HasDefaultSchema(context.GetDefaultSchema());
    }
}