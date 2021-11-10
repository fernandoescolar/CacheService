namespace CacheService.Background;

internal record JobParameters<T>(string Key, CacheOptions Options, Func<CancellationToken, ValueTask<T?>> ValueGetter);
