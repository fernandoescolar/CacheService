#if NET9_0_OR_GREATER
using System.Buffers;

namespace CacheService.Tests.Doubles;

public class DummyBufferDistributedCache : DummyDistributedCache, IBufferDistributedCache
{
    private readonly TaskCompletionSource _written = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int BufferWrites { get; private set; }

    public Task Written => _written.Task;

    public bool TryGet(string key, IBufferWriter<byte> destination)
    {
        var value = Get(key);
        if (value is null)
        {
            return false;
        }

        destination.Write(value);
        return true;
    }

    public ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default)
        => new(TryGet(key, destination));

    public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options)
    {
        // the content of the sequence is valid only during this call
        Set(key, value.ToArray(), options);
        BufferWrites++;
        _written.TrySetResult();
    }

    public ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return default;
    }
}
#endif
