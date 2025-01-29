namespace CacheService.Core;

internal sealed partial class UglyCacheService : ICacheService
{
    private readonly CacheServiceOptions _defaultOptions;
    private readonly IMemoryCache? _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly IJobManager? _jobManager;
    private readonly ICacheSerializer _serializer;
    private readonly ILogger _logger;

    private readonly bool _useDistributedCache;
    private readonly bool _useMemoryCache;
    private readonly bool _useJobManager;

    public UglyCacheService(IOptions<CacheServiceConfiguration> configuration, DelegatedFactories factories, ILogger<UglyCacheService> logger)
    {
        _defaultOptions = configuration.Value.DefaultOptions;
        _memoryCache = factories.MemoryCacheFactory();
        _distributedCache = factories.DistributedCacheFactory();
        _jobManager = factories.JobManagerFactory();
        _serializer = factories.CacheSerializerFactory();
        _logger = logger;

        _useDistributedCache = _distributedCache is not null;
        _useMemoryCache = _memoryCache is not null;
        _useJobManager = _jobManager is not null;
    }

    public async ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions? options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ops = options ?? _defaultOptions;

        var result = default(T);
        AddOrUpdateJob(key, ops, getter);

        if (!(options?.ForceRefresh ?? false))
        {
            if (TryGetFromMemory(key, ref result))
            {
                return result;
            }

            result = await TryGetFromDistributedAsync<T>(key, ops, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        return await TryGetFromSourceAsync<T>(key, getter, ops, cancellationToken);
    }

    public Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_memoryCache is not null)
        {
            _memoryCache.Remove(key);
        }

        if (_distributedCache is not null)
        {
            return _distributedCache.RemoveAsync(key, cancellationToken);
        }

        return Task.CompletedTask;
    }
}
