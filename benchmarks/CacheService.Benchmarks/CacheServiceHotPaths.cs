using System.Buffers;
using System.Diagnostics;

using Microsoft.Extensions.Caching.Distributed;

namespace CacheService.Benchmarks;

/// <summary>
/// Micro benchmarks of the <see cref="ICacheService" /> code paths.
/// They use a real memory cache and a fake distributed cache (it has no cost at all),
/// so they measure the <see cref="ICacheService" /> overhead and no Redis instance is needed to run them:
/// <c>dotnet run -c Release -- --filter *HotPaths*</c>
/// </summary>
[MemoryDiagnoser]
public class CacheServiceHotPaths
{
    private const string Key = "benchmark-key";

    private static readonly Payload Value = Payload.Create();
    private static readonly Func<CancellationToken, ValueTask<Payload?>> Getter = _ => new ValueTask<Payload?>(Value);

    private readonly CacheServiceOptions _options = new();
    private readonly CacheServiceOptions _refreshOptions = new() { Memory = { RefreshInterval = TimeSpan.FromHours(1) }, Distributed = { RefreshInterval = TimeSpan.FromHours(1) } };
    private readonly CacheServiceOptions _forceRefreshOptions = new() { ForceRefresh = true };
    private readonly CacheServiceOptions _distributedOnlyOptions = new() { UseMemoryCache = false };
    private readonly ArrayBufferWriter<byte> _buffer = new(8 * 1024);
    private readonly FakeDistributedCache _distributedCache = new();

    private ServiceProvider? _provider;
    private ICacheService _cacheService = default!;
    private ICacheSerializer _serializer = default!;
    private byte[] _serialized = [];

    [GlobalSetup]
    public async Task Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddSingleton<IDistributedCache>(_distributedCache);
        services.AddCacheService();

        _provider = services.BuildServiceProvider();
        _cacheService = _provider.GetRequiredService<ICacheService>();
        _serializer = _provider.GetRequiredService<ICacheSerializer>();

        _serializer.Serialize(Value, _buffer);
        _serialized = _buffer.WrittenSpan.ToArray();
        _distributedCache.Stored = _serialized;

        await _cacheService.GetOrSetAsync(Key, _options, Getter);
    }

    [GlobalCleanup]
    public void Cleanup()
        => _provider?.Dispose();

    [Benchmark(Description = "Memory hit (default options)")]
    public ValueTask<Payload?> MemoryHit()
        => _cacheService.GetOrSetAsync(Key, Getter);

    [Benchmark(Description = "Memory hit (custom options)")]
    public ValueTask<Payload?> MemoryHitWithOptions()
        => _cacheService.GetOrSetAsync(Key, _options, Getter);

    [Benchmark(Description = "Memory hit (RefreshInterval)")]
    public ValueTask<Payload?> MemoryHitWithRefreshInterval()
        => _cacheService.GetOrSetAsync(Key, _refreshOptions, Getter);

    [Benchmark(Description = "Distributed hit")]
    public ValueTask<Payload?> DistributedHit()
        => _cacheService.GetOrSetAsync(Key, _distributedOnlyOptions, Getter);

    /// <summary>
    /// The distributed cache is written in background, so this one waits until the value has arrived
    /// to measure the whole work (it includes the cost of the hop to the thread pool).
    /// </summary>
    [Benchmark(Description = "Source + write both caches")]
    public async Task<Payload?> ForceRefresh()
    {
        var writes = _distributedCache.Writes;
        var result = await _cacheService.GetOrSetAsync(Key, _forceRefreshOptions, Getter);
        _distributedCache.WaitForWrite(writes);

        return result;
    }

    [Benchmark(Description = "Serializer: serialize")]
    public int Serialize()
    {
        _buffer.ResetWrittenCount();
        _serializer.Serialize(Value, _buffer);
        return _buffer.WrittenCount;
    }

    [Benchmark(Description = "Serializer: deserialize")]
    public Payload? Deserialize()
        => _serializer.Deserialize<Payload>(_serialized);

    public sealed class Payload
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public double Price { get; set; }

        public string[] Tags { get; set; } = [];

        public static Payload Create()
            => new()
            {
                Id = 42,
                Name = "CacheService benchmark payload",
                Description = new string('x', 2_000),
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Price = 1234.56,
                Tags = ["cache", "memory", "distributed", "benchmark"],
            };
    }

    /// <summary>
    /// A distributed cache that always returns the same value and discards the written ones.
    /// Like the Redis one, it implements <see cref="IBufferDistributedCache" />.
    /// </summary>
    private sealed class FakeDistributedCache : IBufferDistributedCache
    {
        private long _writes;

        public byte[]? Stored { get; set; }

        public long Writes => Volatile.Read(ref _writes);

        public void WaitForWrite(long previousWrites)
        {
            var start = Stopwatch.GetTimestamp();
            var spin = new SpinWait();
            while (Writes == previousWrites)
            {
                // never sleeps: a Thread.Sleep(1) would be much longer than the work to measure
                spin.SpinOnce(sleep1Threshold: -1);
                if (Stopwatch.GetElapsedTime(start) > TimeSpan.FromSeconds(10))
                {
                    throw new TimeoutException("The value has not been written in the distributed cache.");
                }
            }
        }

        public byte[]? Get(string key)
            => Stored;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            => Task.FromResult(Stored);

        public bool TryGet(string key, IBufferWriter<byte> destination)
        {
            if (Stored is null)
            {
                return false;
            }

            destination.Write(Stored);
            return true;
        }

        public ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = default)
            => new(TryGet(key, destination));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            => OnWrite(value.Length);

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            OnWrite(value.Length);
            return Task.CompletedTask;
        }

        public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options)
            => OnWrite(value.Length);

        public ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            OnWrite(value.Length);
            return default;
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public void Remove(string key)
        {
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        private void OnWrite(long length)
        {
            if (length == 0)
            {
                throw new InvalidOperationException("Empty value written in the distributed cache.");
            }

            Interlocked.Increment(ref _writes);
        }
    }
}
