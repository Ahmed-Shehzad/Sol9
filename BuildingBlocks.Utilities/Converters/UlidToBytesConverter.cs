using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Utilities.Converters;

/// <summary>
/// Represents a custom value converter for converting <see cref="Ulid"/> to and from a byte array.
/// </summary>
/// <param name="mappingHints">Optional mapping hints for the converter.</param>
public class UlidToBytesConverter(ConverterMappingHints? mappingHints = null) : ValueConverter<Ulid, byte[]>(
    convertToProviderExpression: x => x.ToByteArray(),
    convertFromProviderExpression: x => new Ulid(x),
    mappingHints: DefaultHints.With(mappingHints))
{
    /// <summary>
    /// A static readonly instance of <see cref="ConverterMappingHints"/> with a size of 16.
    /// </summary>
    private static readonly ConverterMappingHints DefaultHints = new(size: 16);

    /// <summary>
    /// Initializes a new instance of the <see cref="UlidToBytesConverter"/> class with default mapping hints.
    /// </summary>
    public UlidToBytesConverter() : this(null)
    {
    }
}