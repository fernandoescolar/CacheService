namespace CacheService.Core;

sealed partial class UglyCacheService
{
    private bool TryGetFromMemory<T>(string key, ref T? result) where T : class
    {
        if (_useMemoryCache)
        {
            #nullable disable
            result = _memoryCache.TryGetValue(key, out var value) ? (T)value : default;
            #nullable restore
            return result is not null;
        }

        return false;
    }

    private void TrySetMemory<T>(string key, CacheServiceOptions ops, T? result) where T : class
    {
        if (_useMemoryCache)
        {
            #nullable disable
            _memoryCache.Set(key, result, ops.Memory);
            #nullable restore
        }
    }
}
