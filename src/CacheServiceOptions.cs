using Microsoft.Extensions.Caching.Distributed;

namespace CacheService
{
    public class CacheServiceOptions : DistributedCacheEntryOptions
    {
        public static readonly CacheServiceOptions Default = new();
    }
}
