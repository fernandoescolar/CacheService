namespace CacheService;

/// <summary>
/// Provides the cache options for an entry.
/// </summary>
public class CacheOptions : DistributedCacheEntryOptions
{
    /// <summary>
    /// Interval to update cached value from source.
    /// </summary>
    public TimeSpan? RefreshInterval { get; set; } = default;
}
