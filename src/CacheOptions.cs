using Microsoft.Extensions.Caching.Distributed;

namespace CacheService
{
    public class CacheOptions : DistributedCacheEntryOptions
    {
        public TimeSpan? RefreshInterval { get; set; } = default;
    }
}
