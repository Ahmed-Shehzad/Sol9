namespace BuildingBlocks.Extensions.Types;

public static class StringExtensions
{
    /// <summary>
    ///     Joins list to single string with separator for not empty members
    /// </summary>
    /// <param name="this"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string JoinIfNotNullOrWhitespace(this IEnumerable<string> @this, char separator)
    {
        return string.Join(separator, @this.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
        
    /// <summary>
    ///     Joins list to single string with separator for not empty members
    /// </summary>
    /// <param name="this"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string JoinIfNotNullOrWhitespace(this IEnumerable<string?> @this, string separator)
    {
        return string.Join(separator, @this.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    /// <summary>
    /// Checks if a given string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="value"/> parameter is null, empty, or consists only of white-space characters; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return value == null || string.IsNullOrWhiteSpace(value);
    }
}