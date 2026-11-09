namespace CacheService.Extensions;

internal static class DistributedCacheExtensions
{
    /// <summary>
    /// Sets the content of a <see cref="PooledBufferWriter" /> in the cache.
    /// </summary>
    /// <remarks>
    /// The buffer has to be alive until the returned task is completed.
    /// </remarks>
    public static Task SetAsync(this IDistributedCache cache, string key, PooledBufferWriter buffer, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
    {
#if NET9_0_OR_GREATER
        if (cache is IBufferDistributedCache bufferCache)
        {
            // it reads directly from the pooled buffer: no need to allocate an array with the payload
            return bufferCache.SetAsync(key, new ReadOnlySequence<byte>(buffer.AsReadOnlyMemory()), options, cancellationToken).AsTask();
        }
#endif

        return cache.SetAsync(key, buffer.ToArray(), options, cancellationToken);
    }
}
