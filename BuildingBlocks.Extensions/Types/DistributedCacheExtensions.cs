using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace BuildingBlocks.Extensions.Types;

public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    /// <summary>
    /// Sets a value in the cache with a default expiration policy.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="cache">The IDistributedCache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to be stored.</param>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
    {
        return SetAsync(cache, key, value, new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1)));
    }

    /// <summary>
    /// Sets a value in the cache with a specified expiration policy.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="cache">The IDistributedCache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to be stored.</param>
    /// <param name="options">The expiration policy options.</param>
    private static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, SerializerOptions));
        return cache.SetAsync(key, bytes, options);
    }

    /// <summary>
    /// Tries to get a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="cache">The IDistributedCache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The output parameter to store the retrieved value.</param>
    /// <returns>True if the value was found in the cache; otherwise, false.</returns>
    private static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
    {
        var val = cache.Get(key);
        
        value = default;
        if (val == null) return false;
        
        var json = Encoding.UTF8.GetString(val);
        value = JsonSerializer.Deserialize<T>(json, SerializerOptions);
        return true;
    }

    /// <summary>
    /// Gets a value from the cache or sets it if not found, with a specified expiration policy.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="cache">The IDistributedCache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="task">The function to retrieve the value if not found in the cache.</param>
    /// <param name="options">The expiration policy options.</param>
    /// <returns>The value from the cache or the result of the task if not found.</returns>
    public static async Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> task,
        DistributedCacheEntryOptions? options = null)
    {
        options ??= new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        if (cache.TryGetValue(key, out T? value) && value is not null)
            return value;

        value = await task();

        if (value is not null)
            await cache.SetAsync(key, value, options);

        return value;
    }
}