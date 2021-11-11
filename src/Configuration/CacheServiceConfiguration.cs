namespace CacheService.Configuration;

/// <summary>
/// Configuration for <see cref="ICacheService" />.
/// </summary>
public class CacheServiceConfiguration
{
    /// <summary>
    /// Gets or sets the <see cref="CacheServiceOptions" /> to use by default in <see cref="ICacheService" /> GetOrSetAsync method.
    /// </summary>
    public CacheServiceOptions DefaultOptions { get; set; } = new CacheServiceOptions();

    /// <summary>
    /// Gets or sets if you want to use <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /> inside <see cref="ICacheService" />.
    /// </summary>
    /// <remarks>Default value is true.</remarks>
    public bool UseMemoryCache { get; set; } = true;

    /// <summary>
    /// Gets or sets if you want to use <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache" /> inside <see cref="ICacheService" />.
    /// </summary>
    /// // <remarks>Default value is <see cref="true" />.</remarks>
    public bool UseDistributedCache { get; set; } = true;

    /// <summary>
    /// Gets or sets how you want to use the background process to automatically update your cache values.
    /// </summary>
    /// <remarks>Default value is <see cref="BackgroundJobMode.HostedService" />.</remarks>
    public BackgroundJobMode BackgroundJobMode { get; set; } = BackgroundJobMode.HostedService;

    /// <summary>
    /// Gets or sets the background process execution interval.
    /// </summary>
    /// <remarks>Default value is One minute.</remarks>
    public TimeSpan BackgroundJobInterval { get; set; } = TimeSpan.FromMinutes(1);
}
