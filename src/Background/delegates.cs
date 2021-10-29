using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CacheService.Background
{
    internal delegate IMemoryCache MemoryCacheFactory();

    internal delegate IDistributedCache DistributedCacheFactory();

    internal delegate ICacheSerializer CacheSerializerFactory();
}
