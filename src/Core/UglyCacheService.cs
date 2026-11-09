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
        _jobManager = factories.JobManagerFactory();
        _serializer = factories.CacheSerializerFactory();
        _logger = logger;

        _useDistributedCache = _defaultOptions.UseDistributedCache;
        _useMemoryCache = _defaultOptions.UseMemoryCache;
        _useJobManager = _jobManager is not null;

        if (_useMemoryCache)
        {
            _memoryCache = factories.MemoryCacheFactory();
        }

        if (_useDistributedCache)
        {
            _distributedCache = factories.DistributedCacheFactory();
        }
    }

    public ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions? options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T : class
    {
        // This method is not async on purpose: when the value is in the memory cache (the hot path)
        // it completes synchronously, so it does not need an async state machine.
        // As it would happen in an async method, the exceptions are returned inside the ValueTask.
        var ops = options ?? _defaultOptions;
        var forceRefresh = options?.ForceRefresh ?? false;

        if (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = default(T);
                AddOrUpdateJob(key, ops, getter);

                if (!forceRefresh && ops.UseMemoryCache && TryGetFromMemory(key, ref result))
                {
                    return new ValueTask<T?>(result);
                }
            }
            catch (Exception ex)
            {
                return ValueTask.FromException<T?>(ex);
            }
        }

        return GetFromDistributedOrSourceAsync(key, ops, forceRefresh, getter, cancellationToken);
    }

    private async ValueTask<T?> GetFromDistributedOrSourceAsync<T>(string key, CacheServiceOptions ops, bool forceRefresh, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken) where T : class
    {
        // when the operation is cancelled nothing has been done yet
        cancellationToken.ThrowIfCancellationRequested();

        if (!forceRefresh && ops.UseDistributedCache)
        {
            var result = await TryGetFromDistributedAsync<T>(key, ops, cancellationToken);
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
