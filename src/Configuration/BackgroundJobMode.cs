namespace CacheService.Configuration;

/// <summary>
/// Cache updater background job mode.
/// </summary>
public enum BackgroundJobMode
{
    /// <summary>
    /// Do not configure any background process.
    /// </summary>
    None,

    /// <summary>
    /// Configure <see cref="Microsoft.Extensions.Hosting.IHostedService" /> background process.
    /// </summary>
    HostedService,

    /// <summary>
    /// Configure a background process based in <see cref="System.Timers.Timer" />.
    /// This process will be automatically started when you try to get or set a value in <see cref="ICacheService" />.
    /// </summary>
    Timer
}
