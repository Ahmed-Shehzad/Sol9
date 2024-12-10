using System.Text.Json;
using System.Text.RegularExpressions;
using BuildingBlocks.Utilities.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Extensions;


    public static partial class NpgsqlModelBuilderExtensions
    {
        public static void UseNpgsqlSpatialData(this ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis");
        }
        
        public static void UseNpgsqlDictionaryConvention(this ModelBuilder builder)
        {
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                foreach(var property in entity.GetProperties().Where(p => typeof(IDictionary<string, string>).IsAssignableFrom(p.ClrType)))
                {
                    var options = JsonConfigurations.GetDefaultOptions();
                    property.SetValueConverter(
                        new ValueConverter<IDictionary<string, string>?, string?>(
                            dict => dict != null ? 
                            JsonSerializer.Serialize(dict, options) : null,
                            str => str != null ? 
                                JsonSerializer.Deserialize<Dictionary<string, string>?>(str, options) : null));
                    property.SetValueComparer(new ValueComparer<IDictionary<string, string>>(
                        (c1, c2) => c2 != null && c1 != null && c1.SequenceEqual(c2), 
                        c => 
                            c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToDictionary(d => d.Key, 
                            d => d.Value)));
                    property.SetColumnType("jsonb");
                }
            }
        }
        
        public static void UseNpgsqlNamingConvention(this ModelBuilder builder)
        {
            foreach(var entity in builder.Model.GetEntityTypes())
            {
                if (entity.BaseType == null) entity.SetTableName(entity.GetTableName().ToSnakeCase());
                foreach (var property in entity.GetProperties()) property.SetColumnName(property.GetColumnName().ToSnakeCase());
                foreach(var key in entity.GetKeys()) key.SetName(key.GetName().ToSnakeCase());
                foreach(var key in entity.GetForeignKeys()) key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                foreach(var index in entity.GetIndexes()) index.SetDatabaseName(ToSnakeCase(index.Name));
            }
        }

        public static void UseSingularTableNamingConvention(this ModelBuilder builder)
        {
            foreach (var entity in builder.Model.GetEntityTypes())
                if (entity.BaseType == null) entity.SetTableName(entity.DisplayName());
        } 

        public static void UseCustomNamingConvention(this ModelBuilder builder)
        {
            foreach(var entity in builder.Model.GetEntityTypes())
            {
                if (entity.BaseType == null) entity.SetTableName(entity.GetTableName().ToFirstLetterLowerCase());
                foreach (var property in entity.GetProperties()) property.SetColumnName(property.GetColumnName().ToFirstLetterLowerCase());
                foreach(var key in entity.GetKeys()) key.SetName(key.GetName().ToSnakeCase());
                foreach(var key in entity.GetForeignKeys()) key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                foreach(var index in entity.GetIndexes()) index.SetDatabaseName(ToSnakeCase(index.Name));
            }
        }

        public static string? ToFirstLetterLowerCase(this string? str)
        {
            if (str != null) return char.ToLower(str[0]) + str[1..];
            return str;
        }
        
        public static string? ToSnakeCase(this string? str)
        {
            if (str == null) return str;
            str = NonUnderscoreFollowedByCapitalizedNameRegex().Replace(str, "$1_$2");
            return LowercaseOrDigitThenUppercasePatternRegex().Replace(str, "$1_$2").ToLower();
        }

    [GeneratedRegex("((?!_).)([A-Z][a-z]+)")]
    private static partial Regex NonUnderscoreFollowedByCapitalizedNameRegex();
    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex LowercaseOrDigitThenUppercasePatternRegex();
}