using BuildingBlocks.Utilities.Types;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Utilities.Converters;

public class EnumerationConverter<T>() : ValueConverter<T, long>(e => e.Id, id => Enumeration.FromId<T>(id))
    where T : Enumeration, new();