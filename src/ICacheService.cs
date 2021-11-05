namespace CacheService
{
    public interface ICacheService
    {
        ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T: class;

        Task InvalidateAsync(string key, CancellationToken cancellationToken = default);
    }
}
