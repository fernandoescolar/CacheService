namespace CacheService
{
    public static class CacheServiceExtensions
    {
        public static Task<T?> GetOrSetAsync<T>(this ICacheService service, string key, Func<CancellationToken, Task<T?>> getter, CancellationToken cancellationToken = default) where T: class
            => service.GetOrSetAsync(key, CacheServiceOptions.Default, getter, cancellationToken);

        public static Task<T?> GetOrSetAsync<T>(this ICacheService service, string key, Func<T?> getter, CancellationToken cancellationToken = default) where T: class
            => service.GetOrSetAsync(key, CacheServiceOptions.Default, getter, cancellationToken);

        public static Task<T?> GetOrSetAsync<T>(this ICacheService service, string key, CacheServiceOptions options, Func<T?> getter, CancellationToken cancellationToken = default) where T: class
            => service.GetOrSetAsync(key, options, ct => Task.FromResult(getter()), cancellationToken);
    }
}
