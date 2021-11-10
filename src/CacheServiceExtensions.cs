namespace CacheService;

public static class CacheServiceExtensions
{
    /// <summary>
    /// Gets or Sets a value with the given key.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="service">The <see cref="ICacheService"/> object.</param>
    /// <param name="key">The key to read/write a value from cache.</param>
    /// <param name="getter">The fucntion that gets the value from source.</param>
    /// <param name="cancellationToken">Optional. The System.Threading.CancellationToken used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The cached/read requested value.</returns>
    public static ValueTask<T?> GetOrSetAsync<T>(this ICacheService service, string key, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T : class
        => service.GetOrSetAsync(key, default, getter, cancellationToken);

    /// <summary>
    /// Gets or Sets a value with the given key.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="service">The <see cref="ICacheService"/> object.</param>
    /// <param name="key">The key to read/write a value from cache.</param>
    /// <param name="getter">The fucntion that gets the value from source.</param>
    /// <param name="cancellationToken">Optional. The System.Threading.CancellationToken used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The cached/read requested value.</returns>
    public static ValueTask<T?> GetOrSetAsync<T>(this ICacheService service, string key, Func<T?> getter, CancellationToken cancellationToken = default) where T : class
        => service.GetOrSetAsync(key, default, ct => ValueTask.FromResult(getter()), cancellationToken);

    /// <summary>
    /// Gets or Sets a value with the given key.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="service">The <see cref="ICacheService"/> object.</param>
    /// <param name="key">The key to read/write a value from cache.</param>
    /// <param name="options">The cache options for this value.</param>
    /// <param name="getter">The fucntion that gets the value from source.</param>
    /// <param name="cancellationToken">Optional. The System.Threading.CancellationToken used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The cached/read requested value.</returns>
    public static ValueTask<T?> GetOrSetAsync<T>(this ICacheService service, string key, CacheServiceOptions options, Func<T?> getter, CancellationToken cancellationToken = default) where T : class
        => service.GetOrSetAsync(key, options, ct => ValueTask.FromResult(getter()), cancellationToken);
}
