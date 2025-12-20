using System.Text;

namespace Transponder;

/// <summary>
/// Builds request addresses using a convention-based path.
/// </summary>
public static class TransponderRequestAddressResolver
{
    public const string DefaultRequestPathPrefix = "requests";

    public static Func<Type, Uri?> Create(
        Uri busAddress,
        string? requestPathPrefix = null,
        Func<Type, string>? pathFormatter = null)
    {
        ArgumentNullException.ThrowIfNull(busAddress);

        var prefix = string.IsNullOrWhiteSpace(requestPathPrefix)
            ? DefaultRequestPathPrefix
            : requestPathPrefix.Trim('/');
        var formatter = pathFormatter ?? DefaultPathFormatter;

        return messageType =>
        {
            if (messageType is null)
            {
                return null;
            }

            var segment = formatter(messageType);
            if (string.IsNullOrWhiteSpace(segment))
            {
                return null;
            }

            var builder = new UriBuilder(busAddress);
            var basePath = builder.Path?.TrimEnd('/') ?? string.Empty;
            builder.Path = $"{basePath}/{prefix}/{segment}";
            return builder.Uri;
        };
    }

    public static string DefaultPathFormatter(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        var raw = messageType.FullName ?? messageType.Name;
        return SanitizeSegment(raw);
    }

    private static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
            {
                builder.Append(ch);
                continue;
            }

            if (ch == '.' || ch == '+' || ch == '/')
            {
                builder.Append('-');
                continue;
            }

            builder.Append('-');
        }

        var normalized = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        return Uri.EscapeDataString(normalized);
    }
}
