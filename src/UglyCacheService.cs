namespace CacheService;

public sealed class UglyCacheService : ICacheService
{
    private readonly CacheServiceConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger _logger;

    public UglyCacheService(IOptions<CacheServiceConfiguration> configuration, IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<UglyCacheService> logger)
    {
        _configuration = configuration.Value;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions? options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        options ??= _configuration.DefaultOptions;
        var result = _memoryCache.TryGetValue(key, out var value) ? (T?)value : default;
        if (result is not null)
        {
            return result;
        }

        try
        {
            var bytes = await _distributedCache.GetAsync(key, cancellationToken);
            if (bytes is not null && bytes.Length > 0)
            {
                result = FastJsonSerializer.Deserialize<T>(bytes);
                if (result is not null)
                {
                    _memoryCache.Set(key, result, options.Memory);
                    return result;
                }
            }
        }
        catch(JsonException jex)
        {
            _logger.CannotDeserializeJson(key, jex.Message);
        }
        catch (Exception ex)
        {
            _logger.CannotSet(key, ex.Message);
        }

        result = await getter(cancellationToken);
        if (result is not null)
        {
            _ = Task.Run(async () => {
                var bytes = FastJsonSerializer.Serialize(result);
                await _distributedCache.SetAsync(key, bytes, options.Distributed, cancellationToken);
            }, cancellationToken);
            _memoryCache.Set(key, result, options.Memory);
        }

        return result;
    }

    public Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _memoryCache.Remove(key);
        return _distributedCache.RemoveAsync(key, cancellationToken);
    }
}
