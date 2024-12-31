using BuildingBlocks.Extensions.Types;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace BuildingBlocks.Extensions.Parsers.JsonElement;

/// <summary>
///     Newtonsoft.JsonElementExtensions Converter for System.Text.JsonElementExtensions JsonElements
/// A custom converter for Newtonsoft.JsonConfigurations.JsonElement to System.Text.JsonConfigurations.JsonElement.
/// This converter allows seamless interoperability between the two JSON libraries.
/// </summary>
public class JsonElementInteroperabilityParser : JsonConverter<System.Text.Json.JsonElement?>
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, System.Text.Json.JsonElement? value, JsonSerializer serializer)
    {
        var text = System.Text.Json.JsonSerializer.Serialize(value);
        if (text.IsNullOrWhiteSpace())
        {
            writer.WriteNull();
            return;
        }
        writer.WriteValue(text);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="hasExistingValue">Whether the existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override System.Text.Json.JsonElement? ReadJson(JsonReader reader, Type objectType, System.Text.Json.JsonElement? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return reader.Value == null ? null : System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement?>((string)reader.Value);
    }
}