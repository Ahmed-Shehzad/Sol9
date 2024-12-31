using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Extensions.Parsers.JsonElement;
using Cysharp.Serialization.Json;

namespace BuildingBlocks.Extensions.Types;

public static class JsonConfigurations
{
    /// <summary>
    ///      Converts a given object to a JsonElement
    /// </summary>
    /// <param name="element"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static JsonElement ToJsonElement(object element, JsonSerializerOptions? options = null)
    {
        options ??= GetDefaultOptions();
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(element, options), options);
    }
    
    /// <summary>
    ///     Merges multiple json elements
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static JsonElement Merge(IEnumerable<JsonElement> elements, JsonSerializerOptions? options = null)
    {
        options ??= GetDefaultOptions();
        var dict = elements
            .SelectMany(e => e.EnumerateObject())
            .ToLookup(t => t.Name, t => t.Value)
            .ToDictionary(t => t.Key, t => t.Last());
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dict, options), options);
    }

    /// <summary>
    ///     Returns a default set of JSON serialization options.
    /// </summary>
    /// <returns>
    ///     A <see cref="JsonSerializerOptions"/> instance with the following settings:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Property naming policy: <see cref="JsonNamingPolicy.CamelCase"/></description>
    ///         </item>
    ///         <item>
    ///             <description>Converters: <see cref="UlidJsonConverter"/>, <see cref="JsonElementParser"/>, <see cref="JsonStringEnumConverter"/></description>
    ///         </item>
    ///         <item>
    ///             <description>Allow trailing commas: true</description>
    ///         </item>
    ///         <item>
    ///             <description>Write indented: true</description>
    ///         </item>
    ///         <item>
    ///             <description>Default ignore condition: <see cref="JsonIgnoreCondition.WhenWritingNull"/></description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static JsonSerializerOptions GetDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true, // Enables case-insensitive matching
            Converters =
            {
                new UlidJsonConverter(),
                new JsonElementParser(),
                new JsonStringEnumConverter(),
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            AllowTrailingCommas = true,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
    }
}