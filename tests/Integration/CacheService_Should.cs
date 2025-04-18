﻿namespace CacheService.Tests.Integration;

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
    public async Task Write_DistributedCache_When_Value_Is_Too_Big()
    {
        var value = new Dictionary<string, TestData>();
        for (var i = 0; i < 1_000; i++)
        {
            var id = i.ToString();
            value[id] = new(id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        await Target.GetOrSetAsync(key, () => value, CancellationToken);

        // Set operation is async, so we need to wait a bit
        await Task.Delay(1_000);

        var distributedValue = DistributedCache[key];

        Assert.NotNull(distributedValue);
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

    [Fact]
    public async Task Replace_Memory_Value_When_Force_Refresh_Option_Is_Set()
    {
        var unexpected = new DummyObject();
        MemoryCache.Add(key, new DummyCacheEntry(key) { Value = unexpected });

        var actual = await Target.GetOrSetAsync(key, new CacheServiceOptions { ForceRefresh = true }, () => expected, CancellationToken);
        Assert.Equal(expected, actual);

        var memoryValue = MemoryCache[key].Value;
        Assert.Equal(expected, memoryValue);
    }

    [Fact]
    public async Task Replace_Distributed_Value_When_Force_Refresh_Option_Is_Set()
    {
        var unexpected = new DummyObject();
        var unexpectedSerialized = System.Text.Encoding.UTF8.GetBytes($@"{{""Id"":""{unexpected.Id}""}}");
        DistributedCache.Add(key, unexpectedSerialized);

        var actual = await Target.GetOrSetAsync(key, new CacheServiceOptions { ForceRefresh = true }, () => expected, CancellationToken);
        Assert.Equal(expected, actual);

        // Set operation is async, so we need to wait a bit
        await Task.Delay(1000);

        var distributedValue = DistributedCache[key];
        Assert.Equal(serialized, distributedValue);
    }

    private sealed record TestData(string Id, string Field1, string Field2);
}
