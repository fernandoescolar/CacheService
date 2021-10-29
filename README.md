# CacheService

Doble layer cache for dotnet

## Features

Workflow `GetOrSet()`:
1. Read from MemoryCache
2. Read from DistributedCache (and set in MemoryCache)
3. Read from source (and set in MemoryCache and DistributedCache)

In background:
1. All values read from any source or cache should automatically updated in background in an specified time

## Use

```chsarp

services.AddMemoryCache();
services.AddRedisDistributedCache(op => ...);
services.AddCacheService(op => ...);

```