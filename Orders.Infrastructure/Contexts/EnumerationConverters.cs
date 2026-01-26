using System.Reflection;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Sol9.Core;

namespace Orders.Infrastructure.Contexts;

public class EnumerationIdConverter<TEnumeration, TId> : ValueConverter<TEnumeration, TId>
    where TEnumeration : Enumeration<TId>
{
    public EnumerationIdConverter() : this(null)
    {
    }

    public EnumerationIdConverter(ConverterMappingHints? mappingHints)
        : base(
            convertToProviderExpression: value => value.Id,
            convertFromProviderExpression: value => EnumerationConverterCache<TEnumeration, TId>.FromId(value),
            mappingHints: mappingHints)
    {
    }
}

internal static class EnumerationConverterCache<TEnumeration, TId> where TEnumeration : Enumeration<TId>
{
    private static readonly MethodInfo GetAllMethod = GetGetAllMethod(typeof(TEnumeration))
        .MakeGenericMethod(typeof(TEnumeration));

    internal static TEnumeration FromId(TId id)
    {
        var values =
            (IEnumerable<TEnumeration>)GetAllMethod.Invoke(null, null)!;

        return values.Single(value => EqualityComparer<TId>.Default.Equals(value.Id, id));
    }

    private static MethodInfo GetGetAllMethod(Type enumerationType)
    {
        for (Type? type = enumerationType; type is not null; type = type.BaseType)
        {
            MethodInfo? method = type.GetMethod("GetAll", BindingFlags.Public | BindingFlags.Static);
            if (method is not null)
                return method;
        }

        throw new InvalidOperationException($"No public static GetAll<T>() method found for '{enumerationType.Name}'.");
    }
}
