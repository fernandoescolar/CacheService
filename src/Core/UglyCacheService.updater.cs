namespace CacheService.Core;

sealed partial class UglyCacheService
{
    private void AddOrUpdateJob<T>(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T?>> getter) where T : class
    {
        if (_useJobManager)
        {
            #nullable disable
            _jobManager.AddOrUpdateJob(key, getter, options);
            #nullable restore
        }
    }
}
