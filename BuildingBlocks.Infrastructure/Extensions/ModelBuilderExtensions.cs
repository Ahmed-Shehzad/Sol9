using System.Linq.Expressions;
using System.Reflection;
using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Utilities.Converters;
using BuildingBlocks.Utilities.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MoreLinq.Extensions;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies enumeration configuration to the specified model builder.
    /// This method iterates through all entity types in the model builder, identifies properties of type <see cref="Enumeration"/>,
    /// and applies a custom value converter to convert these properties to and from their corresponding enumeration values.
    /// </summary>
    /// <param name="modelBuilder">The model builder to apply the enumeration configuration to.</param>
    /// <returns>The same model builder instance with the enumeration configuration applied.</returns>
    public static ModelBuilder ApplyEnumerationConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            foreach (var property in clrType.GetProperties())
            {
                if (!typeof(Enumeration).IsAssignableFrom(property.PropertyType)) continue;
                
                var enumerationType = property.PropertyType;

                var converterType = typeof(EnumerationConverter<>).MakeGenericType(enumerationType);
                var converter = (ValueConverter?)Activator.CreateInstance(converterType);

                if (converter is null) continue;
                
                modelBuilder.Entity(clrType).Property(property.Name).HasConversion(converter);
            }
        }
        return modelBuilder;
    }
    
    /// <summary>
    /// Applies a soft delete query filter to the specified model builder.
    /// This filter will automatically exclude entities marked as deleted from database queries.
    /// </summary>
    /// <param name="modelBuilder">The model builder to apply the filter to.</param>
    /// <remarks>
    /// This method iterates through all entity types in the model builder and checks if they implement the <see cref="IAuditInfo"/> interface.
    /// If the entity type implements <see cref="IAuditInfo"/>, it adds a query filter to exclude entities where the "IsDeleted" property is true.
    /// </remarks>
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
    
    /// <summary>
    /// Applies a tenant-specific query filter to the specified model builder for the given entity type.
    /// This filter will automatically include only entities that belong to the specified tenant in database queries.
    /// </summary>
    /// <param name="modelBuilder">The model builder to apply the filter to.</param>
    /// <param name="tenantId">The unique identifier of the tenant to filter entities by.</param>
    /// <remarks>
    /// This method iterates through all entity types in the model builder and checks if they implement the <see cref="ITenantDependent"/> interface.
    /// If the entity type implements <see cref="ITenantDependent"/>, it adds a query filter to include only entities where the "TenantId" property matches the specified tenantId.
    /// </remarks>
    public static void ApplyTenantQueryFilter(this ModelBuilder modelBuilder, Ulid? tenantId)
    {
        if (tenantId is null) return;
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantDependent).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var propertyMethod = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
            
            if (propertyMethod is null) continue;
            
            var tenantIdProperty  = Expression.Call(propertyMethod, parameter, Expression.Constant("TenantId"));
            
            // Create a filter condition: e.TenantId == tenantId
            var tenantIdConstant = Expression.Constant(tenantId, typeof(Ulid?));
            var filterCondition = Expression.Equal(tenantIdProperty, tenantIdConstant);

            // Build the lambda expression for the query filter
            var filter = Expression.Lambda(filterCondition, parameter);
            
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
    
    /// <summary>
    /// Applies a user-specific query filter to the specified model builder for the given entity type.
    /// This filter will automatically include only entities that belong to the specified user in database queries.
    /// </summary>
    /// <param name="modelBuilder">The model builder to apply the filter to.</param>
    /// <param name="userId">The unique identifier of the user to filter entities by. If null, no filter will be applied.</param>
    /// <remarks>
    /// This method iterates through all entity types in the model builder and checks if they implement the <see cref="IUserDependent"/> interface.
    /// If the entity type implements <see cref="IUserDependent"/>, it adds a query filter to include only entities where the "UserId" property matches the specified userId.
    /// If the userId is null, no filter will be applied, allowing all entities to be retrieved.
    /// </remarks>
    public static void ApplyUserQueryFilter(this ModelBuilder modelBuilder, Ulid? userId)
    {
        if (userId is null) return;
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IUserDependent).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var propertyMethod = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
            
            if (propertyMethod is null) continue;
            
            var userIdProperty  = Expression.Call(propertyMethod, parameter, Expression.Constant("UserId"));
            
            // Create a filter condition: e.UserId == userId
            var userIdConstant = Expression.Constant(userId, typeof(Ulid?));
            var filterCondition = Expression.Equal(userIdProperty, userIdConstant);

            // Build the lambda expression for the query filter
            var filter = Expression.Lambda(filterCondition, parameter);
            
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
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
    
    /// <summary>
    ///     Auto find and apply all IEntityTypeConfiguration to modelBuilder.
    /// </summary>
    public static void ApplyAllConfigurations<TDbContext>(this ModelBuilder modelBuilder)
        where TDbContext: DbContext
    {
        var applyConfigurationMethodInfo = modelBuilder
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));

        typeof(TDbContext).Assembly
            .GetTypes()
            .Select(t => (t,
                i: t.GetInterfaces().FirstOrDefault(i =>
                    i.Name.Equals(typeof(IEntityTypeConfiguration<>).Name, StringComparison.Ordinal))))
            .Where(it => it.i != null)
            .Select(it => (t: it.i?.GetGenericArguments()[0], c: Activator.CreateInstance(it.t)))
            .ForEach(it =>
            {
                if (it.t != null) applyConfigurationMethodInfo.MakeGenericMethod(it.t).Invoke(modelBuilder, [it.c]);
            });
    }

    /// <summary>
    /// Configures the default schema for the database entities in the given model builder.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure the schema for.</param>
    /// <param name="context">The database context to retrieve the default schema from.</param>
    /// <remarks>
    /// This method sets the default schema for all entities in the model builder to the default schema retrieved from the given database context.
    /// </remarks>
    public static void ConfigureDefaultSchema(this ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.HasDefaultSchema(context.GetDefaultSchema());
    }
}