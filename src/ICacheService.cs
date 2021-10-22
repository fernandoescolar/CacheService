namespace CacheService
{
    public interface ICacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, Task<T?>> getter, CancellationToken cancellationToken = default) where T: class;
    }
}
