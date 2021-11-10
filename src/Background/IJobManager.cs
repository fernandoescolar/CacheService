namespace CacheService.Background;

internal interface IJobManager
{
    void AddOrUpdateJob<T>(string key, Func<CancellationToken, ValueTask<T?>> valueGetter, CacheServiceOptions options) where T : class;

    Task ExecuteJobsAsync(CancellationToken cancellationToken);
}
