using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CacheService
{
    public class DefaultCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DefaultCacheService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new();

        public DefaultCacheService(IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<DefaultCacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> getter, CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, CacheServiceOptions.Default, getter, cancellationToken);

        public Task<T?> GetOrSetAsync<T>(string key, Func<T> getter, CancellationToken cancellationToken = default)
            => GetOrSetAsync(key, CacheServiceOptions.Default, getter, cancellationToken);

        public Task<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<T> getter, CancellationToken cancellationToken = default)
           => GetOrSetAsync(key, options, ct => Task.FromResult(getter()), cancellationToken);

        public async Task<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, Task<T>> getter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = GetFromMemoryCache<T>(key);
            if (result is not null)
                return result;


            result = await GetFromDistributedCacheAsync<T>(key, cancellationToken);
            if (result is not null)
            {
                SetInMemoryCache(key, result, options);
                return result;
            }

            result = await GetFromSourceAsync(key, getter, cancellationToken);
            if (result is not null)
            {
                SetInMemoryCache(key, result, options);
                await SetInDistributedCacheAsync(key, result, options, cancellationToken);
                return result;
            }

            return default;
        }

        private T? GetFromMemoryCache<T>(string key)
            => _memoryCache.TryGetValue(key, out var value) ? (T)value : default;

        private async Task<T?> GetFromDistributedCacheAsync<T>(string key, CancellationToken cancellationToken)
        {
            try
            {
                var bytes = await _distributedCache.GetAsync(key, cancellationToken);
                if (bytes is not null && bytes.Length > 0)
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        var result = await JsonSerializer.DeserializeAsync<T>(ms, _jsonSerializerOptions, cancellationToken);
                        if (result is not null)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Invalid json format found reading from DistributedCache with key: {key} -> {ex}");
            }

            return default;
        }

        private async Task<T?> GetFromSourceAsync<T>(string key, Func<CancellationToken, Task<T>> getter, CancellationToken cancellationToken)
        {
            try
            {
                return await getter(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Cannot get from source with key: {key} -> {ex}");
            }

            return default;
        }

        private void SetInMemoryCache<T>(string key, T? value, CacheServiceOptions options)
        {
            using var entry = _memoryCache.CreateEntry(key);
            entry.Value = value;
            entry.AbsoluteExpiration = options.AbsoluteExpiration;
        }

        private async Task SetInDistributedCacheAsync<T>(string key, T? result, CacheServiceOptions options, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, result, _jsonSerializerOptions, cancellationToken);
            await _distributedCache.SetAsync(key, ms.GetBuffer(), options, cancellationToken);
        }
    }
}
