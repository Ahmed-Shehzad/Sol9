using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Transponder.Storage.EntityFrameworkCore.Converters;

public class UlidToStringConverter : ValueConverter<Ulid, string>
{
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 26);

    public UlidToStringConverter() : this(null)
    {
    }

    public UlidToStringConverter(ConverterMappingHints? mappingHints)
        : base(
            convertToProviderExpression: x => x.ToString(),
            convertFromProviderExpression: x => Ulid.Parse(x),
            mappingHints: DefaultHints.With(mappingHints))
    {
    }
}