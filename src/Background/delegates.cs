namespace CacheService.Background;

internal delegate IMemoryCache MemoryCacheFactory();

internal delegate IDistributedCache DistributedCacheFactory();

internal delegate ICacheSerializer CacheSerializerFactory();
