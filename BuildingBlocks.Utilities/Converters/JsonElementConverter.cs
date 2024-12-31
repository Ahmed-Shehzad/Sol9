using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BuildingBlocks.Extensions.Types;

namespace BuildingBlocks.Utilities.Converters;

public class JsonElementConverter()
    : ValueConverter<JsonElement?, string?>(v => ConvertJsonElementToString(v), v => ConvertStringToJsonElement(v))
{

    private static string? ConvertJsonElementToString(JsonElement? jsonElement)
    {
        return jsonElement?.GetRawText();
    }

    private static JsonElement? ConvertStringToJsonElement(string? jsonString)
    {
        if (jsonString.IsNullOrWhiteSpace())
            return null;

        // Parse the JSON string into a JsonDocument
        // Clone the RootElement to avoid modifying the original JsonDocument
        // Use Clone to return a JsonElement
        using var jsonDoc = JsonDocument.Parse(jsonString!);
        return jsonDoc.RootElement.Clone(); // Use Clone to return a JsonElement
    }
}