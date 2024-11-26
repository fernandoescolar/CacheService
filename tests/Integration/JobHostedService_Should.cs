
namespace CacheService.Tests.Integration;

public class JobHostedService_Should : IntegrationTestBase
{
    private readonly string key;
    private readonly CacheServiceOptions options;

    public JobHostedService_Should()
    {
        key = Guid.NewGuid().ToString();
        options = new CacheServiceOptions();
    }

    [Fact]
    public async Task Update_MemoryCache()
    {
        options.Memory.RefreshInterval = TimeSpan.FromSeconds(1);

        var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
        var actual = await TestActAsync();

        Assert.NotEqual(expected, actual);
    }

    [Fact]
    public async Task Update_DistributedCache()
    {
        options.Distributed.RefreshInterval = TimeSpan.FromSeconds(1);

        var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
        MemoryCache.Clear();

        var actual = await TestActAsync();

        Assert.NotEqual(expected, actual);
    }

    [Fact]
    public async Task Update_InvalidatedValue()
    {
        options.Distributed.RefreshInterval = TimeSpan.FromSeconds(1);
        var expected = new DummyObject();
        var actual = await Target.GetOrSetAsync(key, options, () => expected, CancellationToken);

        Assert.Equal(expected, actual);

        await Target.InvalidateAsync(key, CancellationToken);

        actual = await TestActAsync();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Not_Update_MemoryCache()
    {
        options.Memory.RefreshInterval = default;

        var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
        var actual = await TestActAsync();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Not_Update_DistributedCache()
    {
        options.Distributed.RefreshInterval = default;

        var expected = await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
        MemoryCache.Clear();

        var actual = await TestActAsync();

        Assert.Equal(expected, actual);
    }

    private async Task<DummyObject?> TestActAsync()
    {
        await RunJobHostedServiceAsync();

        return await Target.GetOrSetAsync(key, options, () => new DummyObject(), CancellationToken);
    }

    private async Task RunJobHostedServiceAsync()
    {
        if (JobHostedService is null)
        {
            throw new InvalidOperationException("JobHostedService is not initialized.");
        }

        await JobHostedService.StartAsync(CancellationToken);
        await Task.Delay(2500);
        await JobHostedService.StopAsync(CancellationToken);
    }
}
