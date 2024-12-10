using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Utilities.Converters;

/// <summary>
/// Represents a custom value converter for converting <see cref="Ulid"/> to and from string.
/// </summary>
/// <param name="mappingHints">Optional mapping hints for the converter.</param>
public class UlidToStringConverter(ConverterMappingHints? mappingHints = null) : ValueConverter<Ulid, string>(
    convertToProviderExpression: x => x.ToString(),
    convertFromProviderExpression: x => Ulid.Parse(x),
    mappingHints: DefaultHints.With(mappingHints))
{
    /// <summary>
    /// Default mapping hints for the converter.
    /// </summary>
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 26);

    /// <summary>
    /// Initializes a new instance of the <see cref="UlidToStringConverter"/> class with default mapping hints.
    /// </summary>
    public UlidToStringConverter() : this(null)
    {
    }
}