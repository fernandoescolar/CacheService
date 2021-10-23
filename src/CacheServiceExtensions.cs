namespace CacheService
{
    public static class CacheServiceExtensions
    {
        public static ValueTask<T?> GetOrSetAsync<T>(this ICacheService service, string key, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T : class
            => service.GetOrSetAsync(key, CacheServiceOptions.Default, getter, cancellationToken);

        public static ValueTask<T?> GetOrSetAsync<T>(this ICacheService service, string key, Func<T?> getter, CancellationToken cancellationToken = default) where T : class
            => service.GetOrSetAsync(key, CacheServiceOptions.Default, ct => ValueTask.FromResult(getter()), cancellationToken);

        public static ValueTask<T?> GetOrSetAsync<T>(this ICacheService service, string key, CacheServiceOptions options, Func<T?> getter, CancellationToken cancellationToken = default) where T : class
            => service.GetOrSetAsync(key, options, ct => ValueTask.FromResult(getter()), cancellationToken);
    }
}
