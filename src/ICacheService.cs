namespace CacheService
{
    public interface ICacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> getter, CancellationToken cancellationToken = default);
        Task<T?> GetOrSetAsync<T>(string key, Func<T> getter, CancellationToken cancellationToken = default);
        Task<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, Task<T>> getter, CancellationToken cancellationToken = default);
        Task<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<T> getter, CancellationToken cancellationToken = default);
    }
}
