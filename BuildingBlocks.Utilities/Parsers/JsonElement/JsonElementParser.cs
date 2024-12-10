using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Utilities.Parsers.JsonElement;

/// <summary>
/// A custom JSON converter for <see cref="JsonElement"/> that supports nullable values.
/// </summary>
public class JsonElementParser : JsonConverter<System.Text.Json.JsonElement?>
{
    /// <summary>
    /// Writes the JSON representation of the specified value.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to write. Can be null.</param>
    /// <param name="options">The options to be used.</param>
    public override void Write(Utf8JsonWriter writer, System.Text.Json.JsonElement? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            value.Value.WriteTo(writer);
        else
            writer.WriteNullValue();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type of the object to convert to.</param>
    /// <param name="options">The options to be used.</param>
    /// <returns>The object value.</returns>
    /// <remarks>
    /// Note: <see cref="JsonElement"/> cannot be directly deserialized from a string in System.Text.JsonConfigurations.
    /// We need to read it from the reader directly.
    /// </remarks>
    public override System.Text.Json.JsonElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.Clone();
    }
}
