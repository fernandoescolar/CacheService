namespace CacheService
{
    public interface ICacheService
    {
        /// <summary>
        ///  Gets or Sets a value with the given key.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="key">The key to read/write a value from cache.</param>
        /// <param name="options">The cache options for this value.</param>
        /// <param name="getter">The fucntion that gets the value from source.</param>
        /// <param name="cancellationToken">Optional. The System.Threading.CancellationToken used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The cached/read requested value.</returns>
        ValueTask<T?> GetOrSetAsync<T>(string key, CacheServiceOptions? options, Func<CancellationToken, ValueTask<T?>> getter, CancellationToken cancellationToken = default) where T: class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InvalidateAsync(string key, CancellationToken cancellationToken = default);
    }
}
