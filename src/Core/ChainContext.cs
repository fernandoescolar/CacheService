namespace CacheService.Core;

/// <summary>
/// ChainContext is a context object that is passed to each <see cref="IChainLink" /> in the Chain Of Responsibility pattern.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public class ChainContext<T>
{
    /// <summary>
    /// The current cache key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The current <see cref="CacheOptions" />.
    /// </summary>
    public CacheServiceOptions Options { get; }

    /// <summary>
    /// The current not cached value getter.
    /// </summary>
    public Func<CancellationToken, ValueTask<T?>> ValueGetter { get; }

    /// <summary>
    /// The current <see cref="CancellationToken" />.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// The current value to be cached.
    /// </summary>
    public T? Value { get; set; }

    internal ChainContext(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T?>> valueGetter, CancellationToken cancellationToken = default)
    {
        Key = key;
        Options = options;
        ValueGetter = valueGetter;
        CancellationToken = cancellationToken;
    }
}
