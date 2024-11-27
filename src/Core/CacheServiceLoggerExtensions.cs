namespace CacheService.Core;

internal static partial class CacheServiceLoggerExtensions
{
    [LoggerMessage(0, LogLevel.Warning, "Cannot deserialize from json in DistributedCache with key: {Key} -> {jex}")]
    public static partial void CannotDeserializeJson(this ILogger logger, string key, string jex);

    [LoggerMessage(1, LogLevel.Warning, "Cannot set to IDistributedCache with key: {Key} -> {ex}")]
    public static partial void CannotSetDistributedCache(this ILogger logger, string key, string ex);

    [LoggerMessage(2, LogLevel.Warning, "Cannot invalidate to DistributedCache with key: {Key} -> {ex}")]
    public static partial void CannotInvalidateDistributedCache(this ILogger logger, string key, string ex);

    [LoggerMessage(3, LogLevel.Error, "BackgroundJobMode is set to None, no job will be added.")]
    public static partial void BackgroundJobModeIsNone(this ILogger logger);

    [LoggerMessage(4, LogLevel.Error, "Expected IMemoryCache but got null")]
    public static partial void ExpectedIMemoryCacheButGotNull(this ILogger logger);

    [LoggerMessage(5, LogLevel.Error, "Expected IDistributedCache but got null")]
    public static partial void ExpectedIDistributedCacheButGotNull(this ILogger logger);

    [LoggerMessage(6, LogLevel.Warning, "Cannot serialize to json in DistributedCache with key: {Key} -> {jex}")]
    public static partial void CannotSerializeJson(this ILogger logger, string key, string jex);

    [LoggerMessage(7, LogLevel.Error, "Out of memory in DistributedCache with key: {Key} -> {ex}")]
    public static partial void OutOfMemory(this ILogger logger, string key, string ex);

    [LoggerMessage(8, LogLevel.Warning, "Cannot get from IDistributedCache with key: {Key} -> {ex}")]
    public static partial void CannotGetDistributedCache(this ILogger logger, string key, string ex);

}
