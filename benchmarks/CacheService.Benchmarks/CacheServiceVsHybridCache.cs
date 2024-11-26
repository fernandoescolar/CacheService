
namespace CacheService.Benchmarks;

public class CacheServiceVsHybridCache
{
    private const int ConcurrentCalls = 10;
    private static readonly Random Random = new();

    private ICacheService? _cacheService;
    private UglyCacheService? _ugly;
    private HybridCache? _hybrid;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "localhost:5002";
        });

        services.AddCacheService();
        services.AddUglyCacheService();
        #pragma warning disable EXTEXP0018
        services.AddHybridCache();

        var provider = services.BuildServiceProvider();
        _cacheService = provider.GetRequiredService<ICacheService>();
        _ugly = provider.GetRequiredService<UglyCacheService>();
        _hybrid = provider.GetRequiredService<HybridCache>();
    }

    [Benchmark(Baseline = true)]
    public void CacheService()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < ConcurrentCalls; i++)
        {
            var key = GetRandomString(100);
            var value = new { property = GetRandomString(2048) };
            tasks.Add(CacheServiceTask(key, value));
        }

        Task.WhenAll(tasks).Wait();
    }

    [Benchmark()]
    public void UglyService()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < ConcurrentCalls; i++)
        {
            var key = GetRandomString(100);
            var value = new { property = GetRandomString(2048) };
            tasks.Add(UglyTask(key, value));
        }

        Task.WhenAll(tasks).Wait();
    }

    [Benchmark]
    public void HybridCache()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < ConcurrentCalls; i++)
        {
            var key = GetRandomString(100);
            var value = new { property = GetRandomString(2048) };
            tasks.Add(HybridCacheTask(key, value));
        }

        Task.WhenAll(tasks).Wait();
    }

    private async Task CacheServiceTask<T>(string key, T value) where T: class
    {
        if (_cacheService is null)
        {
            throw new InvalidOperationException("Invalid cache");
        }

        var result = await _cacheService.GetOrSetAsync(key, new CacheServiceOptions(), _ => ValueTask.FromResult<T?>(value));
        if (result is null)
        {
            throw new InvalidOperationException("Invalid value");
        }
    }

    private async Task UglyTask<T>(string key, T value) where T: class
    {
        if (_ugly is null)
        {
            throw new InvalidOperationException("Invalid cache");
        }

        var result = await _ugly.GetOrSetAsync(key, new CacheServiceOptions(), _ => ValueTask.FromResult<T?>(value));
        if (result is null)
        {
            throw new InvalidOperationException("Invalid value");
        }
    }

    private async Task HybridCacheTask<T>(string key, T value) where T: class
    {
        if (_hybrid is null)
        {
            throw new InvalidOperationException("Invalid cache");
        }

        var result = await _hybrid.GetOrCreateAsync(key, _ => ValueTask.FromResult<T?>(value));
        if (result is null)
        {
            throw new InvalidOperationException("Invalid value");
        }
    }

    private static string GetRandomString(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}
