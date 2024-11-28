namespace CacheService;

internal delegate IMemoryCache? MemoryCacheFactory();

internal delegate IDistributedCache? DistributedCacheFactory();

internal delegate ICacheSerializer CacheSerializerFactory();

internal delegate IJobManager? JobManagerFactory();

internal class DelegatedFactories
{
    public MemoryCacheFactory MemoryCacheFactory { get; }
    public DistributedCacheFactory DistributedCacheFactory { get; }
    public CacheSerializerFactory CacheSerializerFactory { get; }
    public JobManagerFactory JobManagerFactory { get; }

    public DelegatedFactories(MemoryCacheFactory memoryCacheFactory, DistributedCacheFactory distributedCacheFactory, CacheSerializerFactory cacheSerializerFactory, JobManagerFactory jobManagerFactory)
    {
        MemoryCacheFactory = memoryCacheFactory;
        DistributedCacheFactory = distributedCacheFactory;
        CacheSerializerFactory = cacheSerializerFactory;
        JobManagerFactory = jobManagerFactory;
    }
}