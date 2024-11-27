namespace CacheService.Core;

sealed partial class UglyCacheService
{
    private async ValueTask<T?> TryGetFromSourceAsync<T>(string key, Func<CancellationToken, ValueTask<T?>> getter, CacheServiceOptions ops, CancellationToken cancellationToken) where T : class
    {
        var result = await getter(cancellationToken);
        if (result is not null)
        {
            TrySetMemory(key, ops, result);
            TrySetDistributed(key, ops, result);
        }

        return result;
    }
}
