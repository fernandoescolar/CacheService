using Microsoft.Extensions.Caching.Distributed;

namespace CacheService.Extensions
{
    internal static class DistributeCacheExtensions
    {
        public static void SetAsync<T>(this IDistributedCache cache, string key, T value, CacheOptions options)
        {
            using var entry = cache.CreateEntry(key);
            entry.Value = value;
            entry.AbsoluteExpiration = options.AbsoluteExpiration;
            entry.SlidingExpiration = options.SlidingExpiration;
        }
    }
}
