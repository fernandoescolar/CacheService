using Microsoft.Extensions.Caching.Distributed;

namespace CacheService
{
    public class CacheOptions : DistributedCacheEntryOptions
    {
        /// <summary>
        /// Interval to update cached value from source.
        /// </summary>
        public TimeSpan? RefreshInterval { get; set; } = default;
    }
}
