namespace CacheService.Extensions;

internal static class MemoryCacheExtensions
{
    public static void Set<T>(this IMemoryCache cache, string key, T value, CacheOptions options)
    {
        using var entry = cache.CreateEntry(key);
        entry.Value = value;
        entry.AbsoluteExpiration = options.AbsoluteExpiration;
        entry.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        entry.SlidingExpiration = options.SlidingExpiration;
    }
}
