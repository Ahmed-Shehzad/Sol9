using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Orders.Infrastructure.Contexts;

using Sol9.Core;

namespace Orders.Infrastructure.Configurations;

public static class ConfigurationExtensions
{
    public static PropertyBuilder<TEnum> ConfigureEnumProperty<TEnum>(PropertyBuilder<TEnum> builder) where TEnum : struct, Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

        PropertyBuilder<TEnum> configuredBuilder = underlyingType == typeof(short)
            ? builder.HasConversion<short>().HasColumnType("smallint")
            : underlyingType == typeof(long)
                ? builder.HasConversion<long>().HasColumnType("bigint")
                : builder.HasConversion<int>().HasColumnType("integer");

        return configuredBuilder;
    }

    public static PropertyBuilder<TEnumeration> ConfigureEnumerationProperty<TEnumeration, TId>(PropertyBuilder<TEnumeration> builder)
        where TEnumeration : Enumeration<TId>
    {
        string columnType = GetEnumerationColumnType(typeof(TId));

        return builder
            .HasConversion(new EnumerationIdConverter<TEnumeration, TId>())
            .HasColumnType(columnType);
    }

    private static string GetEnumerationColumnType(Type idType)
    {
        if (idType == typeof(short))
            return "smallint";

        if (idType == typeof(long))
            return "bigint";

        if (idType == typeof(int))
            return "integer";

        if (idType == typeof(string))
            return "nvarchar(max)";

        throw new NotSupportedException($"Enumeration id type '{idType.Name}' is not supported.");
    }
}
