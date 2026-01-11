using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Orders.Infrastructure.Configurations;

public static class ConfigurationExtensions
{
    public static PropertyBuilder<TEnum> ConfigureEnumProperty<TEnum>(PropertyBuilder<TEnum> builder) where TEnum : struct, Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

        return underlyingType == typeof(short)
            ? builder.HasConversion<short>().HasColumnType("smallint")
            : underlyingType == typeof(long) ?
            builder.HasConversion<long>().HasColumnType("bigint") :
            builder.HasConversion<int>().HasColumnType("integer");
    }
}
