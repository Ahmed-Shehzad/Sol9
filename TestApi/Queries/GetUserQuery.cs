using Intercessor.Abstractions;
using TestApi.Dtos;

namespace TestApi.Queries;

public class GetUserQuery : ICachedQuery<UserDto>
{
    public string CacheKey { get; }
    public TimeSpan? CacheDuration { get; }

    public GetUserQuery(string cacheKey, TimeSpan? cacheDuration)
    {
        CacheKey = cacheKey;
        CacheDuration = cacheDuration;
    }
}