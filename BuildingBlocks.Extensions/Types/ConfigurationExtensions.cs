using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Extensions.Types;

public static class ConfigurationExtensions
{
    /// <summary>
    ///     Traverses over the configuration source and expands it.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static object Expand(this IConfiguration configuration)
    {
        var result = new Dictionary<string, object>();
        foreach (var child in configuration.GetChildren())
        {
            result[child.Key] = child.Value ?? Expand(child);
        }
        return result;
    }
}