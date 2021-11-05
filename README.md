# CacheService

Doble layer cache for dotnet

## Features

Workflow `GetOrSet()`:
1. Read from MemoryCache
2. Read from DistributedCache (and set in MemoryCache)
3. Read from source (and set in MemoryCache and DistributedCache)

In background:
1. All values read from any source or cache should automatically updated in background in an specified time

## Quick Start

```bash
dotnet package add CacheService
```

```csharp
// dependencies
services.AddLogging();
services.AddMemoryCache();
services.AddRedisDistributedCache(op => op.ConnectionString = "");

// register cache service
services.AddCacheService();
```

```csharp
var cache = serviceProvider.GetRequiredService<ICacheService>();

var myCachedValue = cache.GetorUpdateAsync("dome-key", cancellationToken => GetValueFromDatabaseAsync(cancellationToken), cancellationToken);
```
## Use

```chsarp
 var services = new ServiceCollection();

services.AddLogging();

services.AddSingleton<IMemoryCache>(MemoryCache);
services.AddSingleton<IDistributedCache>(DistributedCache);

services.AddCacheService();
services.AddCacheService(op => ...);
```

```chsarp
services.AddMemoryCache();
services.AddRedisDistributedCache(op => ...);
services.AddCacheService(op => ...);

```

## License

The source code we develop at CacheService is default being licensed as MIT. You can read more about [here](LICENSE).