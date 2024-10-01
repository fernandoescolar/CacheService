using ConsoleApp;
using CacheService;
using CacheService.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddMemoryCache();
services.AddDistributedMemoryCache();
services.AddCacheService(op =>
{
    op.DefaultOptions.Memory.RefreshInterval = TimeSpan.FromSeconds(10);
    op.DefaultOptions.Distributed.RefreshInterval = TimeSpan.FromSeconds(10);
    op.BackgroundJobMode = BackgroundJobMode.Timer;
    op.BackgroundJobInterval = TimeSpan.FromSeconds(10);
});

var provider = services.BuildServiceProvider();
var cache = provider.GetRequiredService<ICacheService>();
var random = new Random();

var value = await cache.GetOrSetAsync("value", _ => ValueTask.FromResult<Number?>(new Number(random.Next(0, 1000))));
Console.WriteLine($"Value is: {value}");

await Task.Delay(5_000);

value = await cache.GetOrSetAsync("value", _ => ValueTask.FromResult<Number?>(new Number(random.Next(0, 1000))));
Console.WriteLine($"Value is: {value}");

await Task.Delay(10_000);

value = await cache.GetOrSetAsync("value", _ => ValueTask.FromResult<Number?>(new Number(random.Next(0, 1000))));
Console.WriteLine($"Value is: {value}");
