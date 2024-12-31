using System.Text.Json;

namespace BuildingBlocks.Extensions.Types;

public static class ObjectExtensions
{
    /// <summary>
    ///     Easy System.Text.Json serialization
    /// </summary>
    /// <param name="element"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ToString(this object element, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(element, options);
    }
        
    /// <summary>
    ///     Yields a single element as an IEnumerable.
    ///     This method is useful when you want to use LINQ with a single element,
    ///     or when you need to pass a single element to a method that expects an IEnumerable.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <param name="this">The single element to be yielded.</param>
    /// <returns>An IEnumerable containing the single element.</returns>
    public static IEnumerable<T> Yield<T>(this T @this)
    {
        yield return @this;
    }
}