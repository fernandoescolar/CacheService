[![license](https://img.shields.io/badge/License-MIT-purple.svg)](../../LICENSE)
[![version](https://img.shields.io/nuget/vpre/CacheService)](https://www.nuget.org/packages/CacheService)
![This package is compatible with this framework or higher](https://img.shields.io/badge/.Net-8.0-blue)
![This package is compatible with this framework or higher](https://img.shields.io/badge/.Net-9.0-blue)
![release action](https://github.com/fernandoescolar/CacheService/actions/workflows/publish.yaml/badge.svg)
![ci action](https://github.com/fernandoescolar/CacheService/actions/workflows/integration.yaml/badge.svg)

# CacheService

CacheService is a simple and fast double layer cache service for dotnet core.

## Features

The main idea is to have an in memory cache and a distributed cache, both managed by a single service: `ICacheService`.

This service have the `GetOrSetAsync()` method and it should:
1. Read from MemoryCache (if exists return the read value)
2. Read from DistributedCache (if exists return the read value) and then set it in MemoryCache
3. Read from source (if not exists return `null`) and then set in MemoryCache and DistributedCache.

And all values read from any source or cache should be automatically refreshed in the background at a specified time.

![Workflow](doc/workflow.png)

## Quick Start

Before using this library, you need to install the NuGet package:

```bash
dotnet add package CacheService
```

CacheService has some dependencies should be already registered in your application: `ILoggerFactory`, `IMemoryCache` and `IDistributed`. As an example:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;

// dependencies
services.AddLogging();
services.AddMemoryCache();
services.AddStackExchangeRedisCache(op => ...);
```

Next, to use the CacheService you need to add the following line in the startup file:

```csharp
// register cache service
services.AddCacheService();
```

Finally you can use the `ICacheService` in your methods getting the cache from the `ServiceProvider`:

```csharp
var cache = serviceProvider.GetRequiredService<ICacheService>();
var myCachedValue = cache.GetOrSetAsync("some-key", ct => GetValueFromDatabaseAsync(ct), cancellationToken);
```

Or in any Asp.Net Core MVC controller:

```csharp
public WeatherForecastController(ICacheService cache)
{
    _cache = cache;
}

[HttpGet(Name = "GetWeatherForecast")]
public async Task<IActionResult> GetAsync()
{
    var model = await _cache.GetOrSetAsync("forecast", ct => GetFromDatabaseAsync(ct), HttpContext.RequestAborted);
    return Ok(model);
}
```

## ICacheService

The main `ICacheService` method is `GetOrSetAsync`. It allows you to get a value from the cache or set a new value in the cache:

```csharp
ValueTask<T> GetOrSetAsync<T>(string key, CacheServiceOptions options, Func<CancellationToken, ValueTask<T>> getter, CancellationToken cancellationToken = default);
```

The parameters are:
| Parameter | Mandatory | Description | Type |
| --------- | ---------- | ----------- | ---- |
| key | Yes | The cache key | `string` |
| options | No | The `CacheServiceOptions` to use | `CacheServiceOptions` |
| getter | Yes | The function to get the value from the source | `Func<CancellationToken, ValueTask<T>>` |
| cancellationToken | No | The cancellation token | `CancellationToken` |

### CacheServiceOptions

`CacheServiceOptions` is a class that contains the options to use in the `ICacheService` methods. It has the following properties:

| Property | Description | Type |
| -------- | ----------- | ---- |
| Memory | Sets the configuration for the in memory cache | `CacheOptions` |
| Distributed | Sets the configuration for the distributed cache | `CacheOptions` |
| ForceRefresh | Sets if you want to force the refresh of the cache value | `bool` |

### CacheOptions

`CacheOptions` is a class that contains the options to use in each configured kind of cache. It has the following properties:

| Property | Description | Type | Default |
| -------- | ----------- | ---- | ------- |
| AbsoluteExpiration | Sets an absolute expiration date for the cache entry | `DateTimeOffset` | `null` |
| AbsoluteExpirationRelativeToNow | Sets an absolute expiration date relative to now for the cache entry | `TimeSpan` | `null` |
| SlidingExpiration | sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set) | `TimeSpan` | `null` |
| RefreshInterval | Sets the interval to automatically refresh the cache value | `TimeSpan` | `null` |


## Configuration

You can add the cache service to the `services` collection:

```csharp
services.AddCacheService(op => ...);
```

And you can configure the `ICacheService` with the following options:

| Property | Description | Type | Default |
| -------- | ----------- | ---- | ------- |
| DefaultOptions | Sets the default options to use in the `ICacheService` methods | `CacheServiceOptions` | |
| UseMemoryCache | Sets if you want to manage `IMemoryCache` with `ICacheService` | `bool` | `true` |
| UseDistributedCache | Sets if you want to manage `IDistributedCache` with `ICacheService` | `bool` | `true` |
| BackgroundJobMode | Sets how you want to use the background process to automatically update your cache values<br/>*Options are: `None`, `HostedService` or `Timer`* | `BackgroundJobMode` | `BackgroundJobMode.HostedService` |
| BackgroundJobInterval | Sets the background process to update cache value execution interval | `TimeSpan` | `TimeSpan.FromMinutes(1)` |

## Performance vs. HybridCache

The `HybridCache` is a similar library from Microsoft that has a similar goal. The main difference is that `HybridCache` uses a "stampede protection" mechanism to avoid multiple requests to the source when the cache expires. This mechanism is based on a distributed lock that is acquired by the first request that detects that the cache has expired.

The `CacheService` does not have this mechanism, but it has a background process that automatically refreshes the cache value at a specified time. This way, the cache value is always updated and the stampede protection is not necessary.


### Benchmark results

You can see the benchmark project in the `benchmarks/CacheService.Benchmark` folder.

The latest results are:

Microsoft.Extensions.Caching.Hybrid v9.0.0-preview.9.24556.5

Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.100

| Method       | ConcurrentCalls | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Allocated  | Alloc Ratio |
|------------- |---------------- |-----------:|----------:|----------:|------:|--------:|---------:|---------:|-----------:|------------:|
| CacheService | 1               |   184.0 us |   3.13 us |   2.44 us |  1.00 |    0.02 |   1.9531 |   0.4883 |   15.83 KB |        1.00 |
| HybridCache  | 1               |   194.5 us |   3.86 us |   5.78 us |  1.06 |    0.03 |   3.9063 |   0.9766 |   24.58 KB |        1.55 |
|              |                 |            |           |           |       |         |          |          |            |             |
| CacheService | 10              |   553.0 us |  10.96 us |  17.06 us |  1.00 |    0.04 |  19.5313 |   5.8594 |  156.99 KB |        1.00 |
| HybridCache  | 10              |   562.0 us |   9.93 us |   9.28 us |  1.02 |    0.04 |  37.1094 |   9.7656 |   245.1 KB |        1.56 |
|              |                 |            |           |           |       |         |          |          |            |             |
| CacheService | 100             | 3,361.4 us |  63.93 us |  78.51 us |  1.00 |    0.03 | 199.2188 |  78.1250 | 1567.58 KB |        1.00 |
| HybridCache  | 100             | 3,531.9 us |  86.35 us | 172.46 us |  1.05 |    0.06 | 359.3750 |  85.9375 | 2448.26 KB |        1.56 |
|              |                 |            |           |           |       |         |          |          |            |             |
| CacheService | 200             | 6,168.5 us | 120.16 us | 106.52 us |  1.00 |    0.02 | 390.6250 | 171.8750 | 3134.53 KB |        1.00 |
| HybridCache  | 200             | 6,590.8 us | 128.88 us | 143.25 us |  1.07 |    0.03 | 726.5625 | 203.1250 | 4895.26 KB |        1.56 |

## License

The source code we develop at CacheService is default being licensed as MIT. You can read more about [here](LICENSE).