namespace CacheService.Tests.Integration;

public class CacheService_Should : IntegrationTestBase
{
    private readonly string key;
    private readonly DummyObject expected;
    private readonly byte[] serialized;

    public CacheService_Should()
        : base()
    {
        key = Guid.NewGuid().ToString();
        expected = new();
        serialized = System.Text.Encoding.UTF8.GetBytes($@"{{""Id"":""{expected.Id}""}}");
    }

    [Fact]
    public async Task Read_From_MemoryCache()
    {
        MemoryCache.Add(key, new DummyCacheEntry(key) { Value = expected });

        var actual = await Target.GetOrSetAsync(key, () => new DummyObject(), CancellationToken);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Read_From_DistributedCache()
    {
        DistributedCache.Add(key, serialized);

        var actual = await Target.GetOrSetAsync(key, () => new DummyObject(), CancellationToken);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Read_From_ValueGetter()
    {
        var actual = await Target.GetOrSetAsync(key, () => expected, CancellationToken);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Write_MemoryCache_When_Value_Is_Read_From_DistributedCache()
    {
        DistributedCache.Add(key, serialized);

        var actual = await Target.GetOrSetAsync(key, () => new DummyObject(), CancellationToken);
        var memoryValue = MemoryCache[key].Value;

        Assert.Equal(expected, actual);
        Assert.Equal(expected, memoryValue);
    }

    [Fact]
    public async Task Write_MemoryCache_When_Value_Is_Read_From_ValueGetter()
    {
        var actual = await Target.GetOrSetAsync(key, () => expected, CancellationToken);
        var memoryValue = MemoryCache[key].Value;

        Assert.Equal(expected, actual);
        Assert.Equal(expected, memoryValue);
    }

    [Fact]
    public async Task Write_DistributedCache_When_Value_Is_Read_From_ValueGetter()
    {
        await Target.GetOrSetAsync(key, () => expected, CancellationToken);

        // Set operation is async, so we need to wait a bit
        await Task.Delay(1000);

        var distributedValue = DistributedCache[key];

        Assert.Equal(serialized, distributedValue);
    }

    [Fact]
    public async Task Delete_MemoryCache_Value_When_It_Is_Invalidated()
    {
        MemoryCache.Add(key, new DummyCacheEntry(key) { Value = expected });

        await Target.InvalidateAsync(key, CancellationToken);

        Assert.False(MemoryCache.ContainsKey(key));
    }

    [Fact]
    public async Task Delete_DistributedCache_Value_When_It_Is_Invalidated()
    {
        DistributedCache.Add(key, serialized);

        await Target.InvalidateAsync(key, CancellationToken);

        Assert.False(DistributedCache.ContainsKey(key));
    }
}
