#if NET9_0_OR_GREATER
namespace CacheService.Tests.Integration;

public class BufferDistributedCache_Should
{
    private readonly string key = Guid.NewGuid().ToString();
    private readonly DummyObject expected = new();
    private readonly DummyBufferDistributedCache distributedCache = new();

    [Fact]
    public async Task Receive_The_Serialized_Value_When_It_Is_Read_From_ValueGetter()
    {
        await using var sp = CreateServiceProvider();
        var target = sp.GetRequiredService<ICacheService>();

        await target.GetOrSetAsync(key, () => expected);

        // Set operation is async, so we need to wait for it
        await distributedCache.Written.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(1, distributedCache.BufferWrites);
        Assert.Equal(Serialize(expected), distributedCache[key]);
    }

    [Fact]
    public async Task Receive_The_Serialized_Value_When_It_Is_Updated_By_A_Job()
    {
        var options = new CacheServiceOptions { UseMemoryCache = false };
        options.Distributed.RefreshInterval = TimeSpan.FromMilliseconds(1);

        await using var sp = CreateServiceProvider();
        var target = sp.GetRequiredService<ICacheService>();
        var jobManager = sp.GetRequiredService<IJobManager>();

        await target.GetOrSetAsync(key, options, () => expected);
        await distributedCache.Written.WaitAsync(TimeSpan.FromSeconds(5));
        distributedCache.Clear();

        await Task.Delay(50);
        await jobManager.ExecuteJobsAsync(CancellationToken.None);

        Assert.Equal(2, distributedCache.BufferWrites);
        Assert.Equal(Serialize(expected), distributedCache[key]);
    }

    private static byte[] Serialize(DummyObject value)
        => System.Text.Encoding.UTF8.GetBytes($@"{{""Id"":""{value.Id}""}}");

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IMemoryCache>(new DummyMemoryCache());
        services.AddSingleton<IDistributedCache>(distributedCache);
        services.AddCacheService();

        return services.BuildServiceProvider();
    }
}
#endif
